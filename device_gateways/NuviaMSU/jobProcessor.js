import axios from "axios";
import {
  getMeasurements,
  ensureClientAlive,
  setChannelIndex,
} from "./nuviaProtocol.js";

async function sendDataToApi(apiUrl, accessToken, measurements, channelIndex) {
  channelIndex = channelIndex + 1;
  const endpoint = `${apiUrl}/devices/${accessToken}/data`;
  const timeStamp = Math.floor(Date.now() / 1000);
  const payload = [];

  if (measurements.currentCPS !== 0)
    payload.push({
      tag: `currentCPS${channelIndex}`,
      value: measurements.currentCPS * 1000,
      timeStamp,
    });
  if (measurements.currentDP !== -1)
    payload.push({
      tag: `currentDP${channelIndex}`,
      value: measurements.currentDP,
      timeStamp,
    });
  if (measurements.cps !== 0)
    payload.push({
      tag: `cps${channelIndex}`,
      value: measurements.cps * 1000,
      timeStamp,
    });
  if (measurements.dp !== -1)
    payload.push({
      tag: `dp${channelIndex}`,
      value: measurements.dp,
      timeStamp,
    });

  try {
    await axios.post(endpoint, payload);
  } catch (err) {
    console.error(`API post failed for ${accessToken}:`, err.message);
  }
}

async function updateJobStatus(apiUrl, jobId, payload) {
  try {
    await axios.put(`${apiUrl}/jobs/${jobId}`, payload);
  } catch (err) {
    console.error(`Failed to update job ${jobId}:`, err.message);
  }
}

async function getJobStatus(apiUrl, jobId) {
  try {
    const res = await axios.get(`${apiUrl}/jobs/${jobId}`);
    return res.data;
  } catch (err) {
    console.error(`Failed to fetch job ${jobId}:`, err.message);
    return null;
  }
}

async function processJob(client, apiUrl, accessToken, jobId, processingJobs) {
  const job = await getJobStatus(apiUrl, jobId);
  if (!job) {
    console.error(`Job ${jobId} not found.`);
    return;
  }

  processingJobs.add(jobId);

  let currentStep =
    job.currentStep && job.currentStep > 0 ? job.currentStep : 1;
  let currentCycle =
    job.currentCycle && job.currentCycle > 0 ? job.currentCycle : 1;
  const totalSteps = job.totalSteps && job.totalSteps > 0 ? job.totalSteps : 1;
  const totalCycles =
    job.totalCycles && job.totalCycles > 0 ? job.totalCycles : 1;
  const isInfinite = job.isInfinite === true;

  // Initial update
  await updateJobStatus(apiUrl, jobId, {
    name: job.name,
    currentStep,
    totalSteps,
    currentCycle,
    totalCycles,
    paused: false,
    status: "JOB_IN_PROGRESS",
  });

  let jobCanceled = false;

  try {
    let finished = false;

    while (!finished) {
      // Loop over steps
      while (currentStep <= totalSteps) {
        const latestJob = await getJobStatus(apiUrl, jobId);
        client = await ensureClientAlive(client);

        const currentCommand = latestJob.currentCommand;
        const commandJob = latestJob.commands?.[currentStep - 1];

        if (currentCommand === "cmGetDetector") {
          const channelId = commandJob?.params?.[0] ?? 0;
          try {
            await setChannelIndex(client, channelId);
            await new Promise((r) => setTimeout(r, 10));
            const measurements = await getMeasurements(client);
            console.log(
              `Measurements for channel ${channelId}:`,
              JSON.stringify(measurements)
            );
            await sendDataToApi(apiUrl, accessToken, measurements, channelId);
          } catch (error) {
            console.error(`Error in channel ${channelId}:`, error);
          }
        } else if (currentCommand === "sleep") {
          let sleepSeconds = 1;
          if (commandJob && commandJob.params && commandJob.params[0]) {
            sleepSeconds = commandJob.params[0];
          }
          console.log(
            `Processing sleep for job ${jobId}, cycle ${currentCycle}, step ${currentStep}: sleeping for ${sleepSeconds} seconds`
          );
          await new Promise((resolve) =>
            setTimeout(resolve, sleepSeconds * 1000)
          );
        } else {
          console.log(
            `Unknown command for job ${jobId}: ${latestJob.currentCommand}`
          );
        }

        const updatedJob = await getJobStatus(apiUrl, jobId);
        if (updatedJob && updatedJob.status === "JOB_CANCELED") {
          console.log(
            `Job ${jobId} has been canceled. Stopping further processing.`
          );
          jobCanceled = true;
          finished = true;
          break;
        }

        currentStep++;

        // Update job progress
        await updateJobStatus(apiUrl, jobId, {
          name: job.name,
          currentStep,
          totalSteps,
          currentCycle,
          totalCycles,
          paused: false,
          status: "JOB_IN_PROGRESS",
        });

        if (currentStep > totalSteps) {
          break;
        }
      }

      if (jobCanceled) break;

      if (isInfinite) {
        // Reset step counter and increment cycle for infinite jobs
        currentStep = 1;
        await updateJobStatus(apiUrl, jobId, {
          name: job.name,
          currentStep,
          totalSteps,
          currentCycle: 1,
          totalCycles: 1,
          paused: false,
          status: "JOB_IN_PROGRESS",
        });
      } else if (currentCycle < totalCycles) {
        // For normal jobs, proceed to next cycle if any
        currentCycle++;
        currentStep = 1;
        await updateJobStatus(apiUrl, jobId, {
          name: job.name,
          currentStep,
          totalSteps,
          currentCycle,
          totalCycles,
          paused: false,
          status: "JOB_IN_PROGRESS",
        });
      } else {
        finished = true;
      }
    }

    if (jobCanceled) {
      await updateJobStatus(apiUrl, jobId, {
        name: job.name,
        currentStep,
        totalSteps,
        currentCycle,
        totalCycles,
        paused: false,
        status: "JOB_CANCELED",
      });
    } else {
      await updateJobStatus(apiUrl, jobId, {
        name: job.name,
        currentStep: totalSteps,
        totalSteps,
        currentCycle,
        totalCycles,
        paused: false,
        status: "JOB_SUCCEEDED",
      });
    }
  } catch (error) {
    await updateJobStatus(apiUrl, jobId, { status: "JOB_FAILED" });
    console.error(`Error processing job ${jobId}:`, error);
  } finally {
    processingJobs.delete(jobId);
  }
}

export async function pollJobs(
  client,
  apiUrl,
  accessToken,
  refreshTime,
  cancelToken
) {
  const processingJobs = new Set();

  while (cancelToken.active) {
    try {
      const res = await axios.get(
        `${apiUrl}/devices/${accessToken}/jobs/active`
      );
      const jobs = res.data;

      for (const job of jobs) {
        if (
          ["JOB_QUEUED", "JOB_IN_PROGRESS"].includes(job.status) &&
          !processingJobs.has(job.id)
        ) {
          processJob(client, apiUrl, accessToken, job.id, processingJobs);
        }
      }
    } catch (err) {
      console.error(`Job polling error for ${accessToken}:`, err.message);
    }

    await new Promise((r) => setTimeout(r, refreshTime));
  }
}
