import fs from "fs/promises";
import net from "net";
import axios from "axios";

const CONFIG_PATH = "config/config.json";

// Load configuration (API url, list of devices, and jobRefreshTime)
async function loadConfig() {
  const data = await fs.readFile(CONFIG_PATH, "utf8");
  return JSON.parse(data);
}

// Create a TCP connection to the device (address in "host:port" format)
function createDeviceConnection(address) {
  return new Promise((resolve, reject) => {
    const [host, port] = address.split(":");
    const client = new net.Socket();
    client.connect(parseInt(port, 10), host, () => resolve(client));
    client.on("error", (err) => reject(err));
  });
}

// Helper: calculate checksum as two's complement (8-bit)
function calculateChecksum(buffer) {
  let sum = 0;
  for (const byte of buffer) {
    sum = (sum + byte) & 0xff; // drop any carry beyond 8 bits
  }
  return (~sum + 1) & 0xff;
}

function buildPacket(devAdr, cmd, payload) {
  // Data part: DevAdr, Cmd, and the payload.
  const dataPart = Buffer.concat([Buffer.from([devAdr, cmd]), payload]);

  // NextB: 3 bytes representing the total length of dataPart + 1 (for the checksum)
  const length = dataPart.length + 1;
  const nextB = Buffer.alloc(3);
  nextB.writeUIntBE(length, 0, 3); // big-endian 3-byte length

  // Concatenate NextB and dataPart (this is what will be summed for the checksum)
  const packetWithoutChecksum = Buffer.concat([nextB, dataPart]);

  // Compute checksum over the entire packetWithoutChecksum.
  const checksum = calculateChecksum(packetWithoutChecksum);

  // Final packet: everything plus the checksum at the end.
  return Buffer.concat([packetWithoutChecksum, Buffer.from([checksum])]);
}

// Send a packet over the socket and wait for response.
// For simplicity we assume the device returns the full answer in one burst.
function sendCommand(client, packet) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    client.once("data", (data) => {
      chunks.push(data);
      // In a real implementation you would check the "NextB" field to know how many bytes to expect.
      setTimeout(() => {
        resolve(Buffer.concat(chunks));
      }, 100);
    });
    client.write(packet, (err) => {
      if (err) {
        reject(err);
      }
    });
  });
}

function extractMeasurements(buffer) {
  // Remove header (first 5 bytes: 3 bytes NextB, 1 byte devAdr, 1 byte cmd)
  // and drop the last checksum byte.
  const payload = buffer.slice(5, buffer.length - 1);
  // Ensure we have at least the expected number of bytes (241 bytes)
  if (payload.length < 241) {
    throw new Error("Measurement payload is too short.");
  }
  // Offsets for detector 4 (0-indexed within payload):
  // - CurrentDP4: bytes 210-213 => offset 209
  // - CPS4: bytes 218-221 => offset 217
  // - DP4: bytes 222-225 => offset 221
  const currentDP4 = payload.readFloatLE(209);
  const cps4 = payload.readFloatLE(217);
  const dp4 = payload.readFloatLE(221);

  return { currentDP: currentDP4, cps: cps4, dp: dp4 };
}

async function getDetectorData(client) {
  const devAdr = 0x00;
  const cmd = 0xbe; // command code for cmGetDetector
  const payload = Buffer.from([0x00]); // reserved byte (set to zero)
  const packet = buildPacket(devAdr, cmd, payload);
  const response = await sendCommand(client, packet);

  return response;
}

async function linkDevice(client) {
  const devAdr = 0x00;
  const cmd = 0x01; // cmLink command code
  const payload = Buffer.alloc(0); // No data bytes are sent
  const packet = buildPacket(devAdr, cmd, payload);
  const response = await sendCommand(client, packet);
  const firmwareBuffer = response.slice(5, response.length - 1);
  const firmware = firmwareBuffer.toString("utf8");
  console.log("Firmware:", firmware);
  return firmware;
}

/**
 * Helper function to check if the connection socket is still alive.
 * If not, it reconnects using the device's address (stored on the socket)
 * and sends the cmLink command again.
 */
async function ensureClientAlive(client) {
  // Check if client exists and is still connected.
  if (!client || client.destroyed) {
    console.log(
      `Socket is not alive for device ${
        client && client.accessToken ? client.accessToken : "unknown"
      }. Reconnecting...`
    );
    if (!client || !client.deviceAddress) {
      throw new Error(
        "Cannot reconnect because device address is unavailable."
      );
    }
    const newClient = await createDeviceConnection(client.deviceAddress);
    // Reattach device info so future reconnection checks work.
    newClient.deviceAddress = client.deviceAddress;
    newClient.accessToken = client.accessToken;
    console.log(
      `Reconnected to device ${newClient.accessToken}. Sending cmLink...`
    );
    await linkDevice(newClient);
    return newClient;
  }
  return client;
}

async function sendDataToApi(apiUrl, accessToken, measurements) {
  const endpoint = `${apiUrl}/devices/${accessToken}/data`;
  const timeStamp = Math.floor(Date.now() / 1000);
  const payload = [
    { tag: "CurrentDP4", value: measurements.currentDP, timeStamp },
    { tag: "CPS4", value: measurements.cps, timeStamp },
    { tag: "DP4", value: measurements.dp, timeStamp },
  ];

  try {
    await axios.post(endpoint, payload);
  } catch (error) {
    console.error(
      `Error sending data to API for device ${accessToken}:`,
      error.message
    );
  }
}

// Helper to update job status via PUT /jobs/{jobId}
async function updateJobStatus(apiUrl, jobId, payload) {
  const endpoint = `${apiUrl}/jobs/${jobId}`;
  try {
    await axios.put(endpoint, payload);
  } catch (error) {
    console.error(`Error updating job ${jobId}:`, error.message);
  }
}

// Helper to fetch active jobs and find a specific job by id
async function getJobStatus(apiUrl, accessToken, jobId) {
  try {
    const endpoint = `${apiUrl}/devices/${accessToken}/jobs/active`;
    const response = await axios.get(endpoint);
    const jobs = response.data;
    return jobs.find((j) => j.id === jobId);
  } catch (error) {
    console.error(`Error fetching job status for ${jobId}:`, error.message);
    return null;
  }
}

// Process a single job command asynchronously.
// The function uses the passed-in "processingJobs" Set to avoid duplicate handling.
async function processJob(client, apiUrl, accessToken, job, processingJobs) {
  const jobId = job.id;
  processingJobs.add(jobId);

  let currentStep =
    job.currentStep && job.currentStep > 0 ? job.currentStep : 1;
  let currentCycle =
    job.currentCycle && job.currentCycle > 0 ? job.currentCycle : 1;
  const totalSteps = job.totalSteps && job.totalSteps > 0 ? job.totalSteps : 1;
  const totalCycles =
    job.totalCycles && job.totalCycles > 0 ? job.totalCycles : 1;

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

  try {
    let finished = false;
    // Outer loop: cycles
    while (currentCycle <= totalCycles && !finished) {
      // Inner loop: steps
      while (currentStep <= totalSteps) {
        // Check for cancellation or pause by refetching job state
        const latestJob = await getJobStatus(apiUrl, accessToken, jobId);
        if (
          !latestJob ||
          latestJob.paused ||
          latestJob.status !== "JOB_IN_PROGRESS"
        ) {
          console.log(
            `Job ${jobId} has been paused or cancelled. Aborting processing.`
          );
          return;
        }

        // *** Ensure the connection is alive before running any command ***
        client = await ensureClientAlive(client);

        // Execute command based on currentCommand
        if (latestJob.currentCommand === "cmGetDetector") {
          console.log(
            `Processing cmGetDetector for job ${jobId}, cycle ${currentCycle}, step ${currentStep}`
          );
          const detectorDataBuffer = await getDetectorData(client);
          const measurements = extractMeasurements(detectorDataBuffer);
          await sendDataToApi(apiUrl, accessToken, measurements);
        } else if (latestJob.currentCommand === "sleep") {
          // Determine sleep parameter from the commands array using currentStep-1
          const commandIndex = currentStep - 1;
          let sleepSeconds = 1; // default to 1 second
          if (
            latestJob.commands &&
            latestJob.commands.length > commandIndex &&
            latestJob.commands[commandIndex].name === "sleep" &&
            latestJob.commands[commandIndex].params &&
            latestJob.commands[commandIndex].params.length > 0
          ) {
            sleepSeconds = latestJob.commands[commandIndex].params[0];
          } else {
            console.warn(
              `Job ${jobId} sleep command at step ${currentStep} missing parameter, defaulting to 1 second.`
            );
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

        // Check if we are at the final step of the final cycle.
        if (currentStep === totalSteps && currentCycle === totalCycles) {
          finished = true;
          break; // Exit the inner loop without incrementing
        } else {
          // Increment step if not at the final command.
          currentStep++;
        }

        // Update job progress after each step.
        await updateJobStatus(apiUrl, jobId, {
          name: job.name,
          currentStep,
          totalSteps,
          currentCycle,
          totalCycles,
          paused: false,
          status: "JOB_IN_PROGRESS",
        });
      }

      // If not finished and there are more cycles, reset step counter and increment cycle.
      if (!finished && currentCycle < totalCycles) {
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
        break;
      }
    }

    // Mark the job as succeeded if finished.
    await updateJobStatus(apiUrl, jobId, {
      name: job.name,
      currentStep: totalSteps,
      totalSteps,
      currentCycle: totalCycles,
      totalCycles,
      paused: false,
      status: "JOB_SUCCEEDED",
    });
  } catch (error) {
    await updateJobStatus(apiUrl, jobId, { status: "JOB_FAILED" });
    console.error(`Error processing job ${jobId}:`, error);
  } finally {
    processingJobs.delete(jobId);
  }
}

// Poll for active jobs and process them concurrently.
// The "processingJobs" Set keeps track of jobs already being handled.
async function pollJobs(client, apiUrl, accessToken, refreshTime) {
  const processingJobs = new Set();
  while (true) {
    try {
      const endpoint = `${apiUrl}/devices/${accessToken}/jobs/active`;
      const response = await axios.get(endpoint);
      const jobs = response.data;
      for (const job of jobs) {
        if (job.status === "JOB_QUEUED" && !processingJobs.has(job.id)) {
          console.log(`Found queued job ${job.id} for device ${accessToken}`);
          // Start processing the job concurrently (do not await so that multiple jobs can run)
          processJob(client, apiUrl, accessToken, job, processingJobs);
        }
      }
    } catch (error) {
      console.error(
        `Error fetching jobs for device ${accessToken}:`,
        error.message
      );
    }
    // Wait for the configured refresh interval before polling again.
    await new Promise((resolve) => setTimeout(resolve, refreshTime));
  }
}

// Process device: connect, link, and start polling for jobs
async function processDevice(device, apiUrl, jobRefreshTime) {
  const { address, accessToken } = device;
  let client;
  try {
    client = await createDeviceConnection(address);
    // Store device info on the socket for future reconnection checks.
    client.deviceAddress = address;
    client.accessToken = accessToken;
    console.log(`Connected to device at ${address}`);
    const firmware = await linkDevice(client);
    console.log(`Device ${accessToken} firmware: ${firmware}`);

    // Start polling for jobs (this runs continuously)
    pollJobs(client, apiUrl, accessToken, jobRefreshTime);
  } catch (error) {
    console.error(`Error processing device ${accessToken}:`, error);
    if (client) client.end();
  }
}

export async function startGateway() {
  try {
    const config = await loadConfig();
    const { api_url: apiUrl, devices, jobRefreshTime } = config;
    // Default refresh time if not set (in milliseconds)
    const refreshTime = jobRefreshTime || 5000;
    await Promise.all(
      devices.map((device) => processDevice(device, apiUrl, refreshTime))
    );
  } catch (error) {
    console.error("Error in main:", error);
  }
}
