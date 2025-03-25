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
      throw new Error("Cannot reconnect because device address is unavailable.");
    }
    const newClient = await createDeviceConnection(client.deviceAddress);
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
        const latestJob = await getJobStatus(apiUrl, accessToken, jobId);
        client = await ensureClientAlive(client);

        if (latestJob.currentCommand === "cmGetDetector") {
          console.log(
            `Processing cmGetDetector for job ${jobId}, cycle ${currentCycle}, step ${currentStep}`
          );
          const detectorDataPacket = await getDetectorData(client);
          const measurements = extractMeasurements(detectorDataPacket);
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

        // Check if we are at the final step of the final cycle.
        if (currentStep === totalSteps && currentCycle === totalCycles) {
          finished = true;
          break;
        } else {
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
async function pollJobs(client, apiUrl, accessToken, refreshTime, cancelToken) {
  while (cancelToken.active) {
    try {
      const endpoint = `${apiUrl}/devices/${accessToken}/jobs/active`;
      const response = await axios.get(endpoint);
      const jobs = response.data;
      for (const job of jobs) {
        // Process job if queued, etc.
        // (Assume processJob and related functions are defined as before.)
        if (job.status === "JOB_QUEUED") {
          processJob(client, apiUrl, accessToken, job, new Set());
        }
      }
    } catch (error) {
      console.error(`Error fetching jobs for device ${accessToken}:`, error.message);
    }
    // Wait for the configured refresh interval before polling again.
    await new Promise((resolve) => setTimeout(resolve, refreshTime));
  }
}

// Process device: connect, link, and start polling for jobs
async function processDevice(device, apiUrl, refreshTime, cancelToken) {
  const { address, accessToken } = device;
  let client;
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
