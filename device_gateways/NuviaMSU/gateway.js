import fs from "fs/promises";
import net from "net";
import axios from "axios";

const activePollers = new Map();
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

// Build packet using the specified format: NextB (3 bytes), DevAdr, Cmd, Data, Checksum
function buildPacket(devAdr, cmd, payload) {
  const dataPart = Buffer.concat([Buffer.from([devAdr, cmd]), payload]);
  const length = dataPart.length + 1; // Data part length plus 1 byte for checksum
  const nextB = Buffer.alloc(3);
  nextB.writeUIntBE(length, 0, 3); // big-endian 3-byte length
  const packetWithoutChecksum = Buffer.concat([nextB, dataPart]);
  const checksum = calculateChecksum(packetWithoutChecksum);
  return Buffer.concat([packetWithoutChecksum, Buffer.from([checksum])]);
}

// Read the complete packet from the client using the NextB field
function readPacket(client) {
  return new Promise((resolve, reject) => {
    let buffer = Buffer.alloc(0);
    function onData(chunk) {
      buffer = Buffer.concat([buffer, chunk]);
      // Ensure we have at least the 3 bytes for NextB
      if (buffer.length >= 3) {
        const expectedLength = 3 + buffer.readUIntBE(0, 3); // total packet length
        if (buffer.length >= expectedLength) {
          client.removeListener("data", onData);
          const packet = buffer.slice(0, expectedLength);
          resolve(packet);
        }
      }
    }
    client.on("data", onData);
    client.on("error", (err) => {
      client.removeListener("data", onData);
      reject(err);
    });
  });
}

// Parse a received packet according to the defined format
function parsePacket(buffer) {
  if (buffer.length < 5) {
    throw new Error("Packet too short.");
  }
  const nextB = buffer.readUIntBE(0, 3);
  if (buffer.length !== 3 + nextB) {
    throw new Error("Invalid packet length.");
  }
  const devAdr = buffer[3];
  const cmd = buffer[4];
  const payload = buffer.slice(5, buffer.length - 1);
  const checksum = buffer[buffer.length - 1];
  const calculated = calculateChecksum(buffer.slice(0, buffer.length - 1));
  if (checksum !== calculated) {
    throw new Error("Checksum mismatch.");
  }
  return { nextB, devAdr, cmd, payload, checksum };
}

// Send a packet over the socket and wait for the complete, parsed response packet.
async function sendCommand(client, packet) {
  return new Promise((resolve, reject) => {
    client.write(packet, (err) => {
      if (err) {
        return reject(err);
      }
      readPacket(client)
        .then((rawPacket) => {
          try {
            const parsed = parsePacket(rawPacket);
            resolve(parsed);
          } catch (error) {
            reject(error);
          }
        })
        .catch(reject);
    });
  });
}

// Extract measurements from the parsed packet's payload.
// This function supports both a parsed packet (with a 'payload' field)
// or a raw packet buffer.
function extractMeasurements(packetOrPayload) {
  const payload =
    packetOrPayload.payload !== undefined
      ? packetOrPayload.payload
      : packetOrPayload.slice(5, packetOrPayload.length - 1);
  if (payload.length < 241) {
    throw new Error("Measurement payload is too short.");
  }

  const currentCPS1 = payload.readFloatLE(64);
  const currentDP1 = payload.readFloatLE(68);
  const cps1 = payload.readFloatLE(76);
  const dp1 = payload.readFloatLE(80);

  const currentCPS2 = payload.readFloatLE(111);
  const currentDP2 = payload.readFloatLE(115);
  const cps2 = payload.readFloatLE(123);
  const dp2 = payload.readFloatLE(127);

  const currentCPS3 = payload.readFloatLE(158);
  const currentDP3 = payload.readFloatLE(162);
  const cps3 = payload.readFloatLE(170);
  const dp3 = payload.readFloatLE(174);

  const currentCPS4 = payload.readFloatLE(205);
  const currentDP4 = payload.readFloatLE(209);
  const cps4 = payload.readFloatLE(217);
  const dp4 = payload.readFloatLE(221);

  return [
    { currentCPS: currentCPS1, currentDP: currentDP1, cps: cps1, dp: dp1 },
    { currentCPS: currentCPS2, currentDP: currentDP2, cps: cps2, dp: dp2 },
    { currentCPS: currentCPS3, currentDP: currentDP3, cps: cps3, dp: dp3 },
    { currentCPS: currentCPS4, currentDP: currentDP4, cps: cps4, dp: dp4 },
  ];
}

async function getDetectorData(client) {
  const devAdr = 0x00;
  const cmd = 0xbe; // command code for cmGetDetector
  const payload = Buffer.from([0x00]); // reserved byte (set to zero)
  const packet = buildPacket(devAdr, cmd, payload);
  const parsedResponse = await sendCommand(client, packet);
  return parsedResponse;
}

async function linkDevice(client) {
  const devAdr = 0x00;
  const cmd = 0x01; // cmLink command code
  const payload = Buffer.alloc(0); // No data bytes are sent
  const packet = buildPacket(devAdr, cmd, payload);
  const parsedResponse = await sendCommand(client, packet);
  const firmwareBuffer = parsedResponse.payload;
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
    newClient.deviceAddress = client.deviceAddress;
    newClient.accessToken = client.accessToken;
    console.log(`Reconnected to device ${newClient.accessToken}.`);
    // await linkDevice(newClient);
    return newClient;
  }
  return client;
}

async function sendDataToApi(apiUrl, accessToken, measurements) {
  const endpoint = `${apiUrl}/devices/${accessToken}/data`;
  const timeStamp = Math.floor(Date.now() / 1000);

  const payload = [];

  // Loop through each measurement and check conditions
  measurements.forEach((measurement, index) => {
    if (measurement.currentCPS !== 0) {
      payload.push({
        tag: `currentCPS${index + 1}`,
        value: measurement.currentCPS * 1000,
        timeStamp,
      });
    }

    if (measurement.currentDP !== -1) {
      payload.push({
        tag: `currentDP${index + 1}`,
        value: measurement.currentDP * 1000,
        timeStamp,
      });
    }

    if (measurement.cps !== 0) {
      payload.push({
        tag: `cps${index + 1}`,
        value: measurement.cps * 1000,
        timeStamp,
      });
    }

    if (measurement.dp !== -1) {
      payload.push({
        tag: `dp${index + 1}`,
        value: measurement.dp * 1000,
        timeStamp,
      });
    }
  });

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
async function getJobStatus(apiUrl, jobId) {
  try {
    const endpoint = `${apiUrl}/jobs/${jobId}`;
    const response = await axios.get(endpoint);
    return response.data;
  } catch (error) {
    console.error(`Error fetching job status for ${jobId}:`, error.message);
    return null;
  }
}

// Process a single job command asynchronously.
// The function uses the passed-in "processingJobs" Set to avoid duplicate handling.
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

        if (latestJob.currentCommand === "cmGetDetector") {
          console.log(
            `Processing cmGetDetector for job ${jobId}, cycle ${currentCycle}, step ${currentStep}`
          );
          const detectorDataPacket = await getDetectorData(client);
          const measurements = extractMeasurements(detectorDataPacket);
          console.log(`Extracted measurements for job ${jobId}:`, measurements);
          await sendDataToApi(apiUrl, accessToken, measurements);
        } else if (latestJob.currentCommand === "sleep") {
          const commandIndex = currentStep - 1;
          let sleepSeconds = 1;
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

// Poll for active jobs and process them concurrently.
async function pollJobs(client, apiUrl, accessToken, refreshTime, cancelToken) {
  const processingJobs = new Set(); // Shared tracking set per device

  while (cancelToken.active) {
    try {
      const endpoint = `${apiUrl}/devices/${accessToken}/jobs/active`;
      const response = await axios.get(endpoint);
      const jobs = response.data;

      for (const job of jobs) {
        if (
          (job.status === "JOB_QUEUED" || job.status === "JOB_IN_PROGRESS") &&
          !processingJobs.has(job.id)
        ) {
          processJob(client, apiUrl, accessToken, job.id, processingJobs);
        }
      }
    } catch (error) {
      console.error(
        `Error fetching jobs for device ${accessToken}:`,
        error.message
      );
    }
    await new Promise((resolve) => setTimeout(resolve, refreshTime));
  }
}

// Process device: connect, link, and start polling for jobs
async function processDevice(device, apiUrl, refreshTime, cancelToken) {
  const { address, accessToken } = device;
  let client;
  console.log(`Processing device ${accessToken} at ${address}`);
  try {
    client = await createDeviceConnection(address);
    client.deviceAddress = address;
    client.accessToken = accessToken;
    console.log(`Connected to device at ${address}`);
    const firmware = await linkDevice(client);
    console.log(`Device ${accessToken} firmware: ${firmware}`);
    // Start polling with cancellation support.
    pollJobs(client, apiUrl, accessToken, refreshTime, cancelToken);
  } catch (error) {
    console.error(`Error processing device ${accessToken}:`, error);
    if (client) client.end();
  }
}

export async function startGateway() {
  console.log("Starting gateway...");
  const config = await loadConfig();
  const { api_url: apiUrl, devices, jobRefreshTime } = config;
  devices.forEach((device) => {
    if (!activePollers.has(device.accessToken)) {
      // Each device gets a cancellation token object.
      const cancelToken = { active: true };
      processDevice(device, apiUrl, jobRefreshTime, cancelToken);
      activePollers.set(device.accessToken, cancelToken);
    }
  });
}

export async function reloadGateway() {
  console.log("Reloading gateway with new configuration...");
  // Cancel all active polling tasks.
  activePollers.forEach((cancelToken, accessToken) => {
    cancelToken.active = false;
    activePollers.delete(accessToken);
  });
  // Restart gateway with new configuration.
  await startGateway();
}
