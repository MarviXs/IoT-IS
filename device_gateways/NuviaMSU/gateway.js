import fs from "fs/promises";
import {
  createDeviceConnection,
  linkDevice,
  getDeviceDescription,
  getDeviceParameters,
  setDeviceParameter,
} from "./nuviaProtocol.js";
import { pollJobs } from "./jobProcessor.js";

const activePollers = new Map();
const CONFIG_PATH = "config/config.json";

async function loadConfig() {
  const data = await fs.readFile(CONFIG_PATH, "utf8");
  return JSON.parse(data);
}

async function processDevice(device, apiUrl, refreshTime, cancelToken) {
  const { address, accessToken } = device;
  let client;

  try {
    client = await createDeviceConnection(address);
    client.deviceAddress = address;
    client.accessToken = accessToken;
    console.log(`Connected to device ${accessToken}`);
    await linkDevice(client);
    pollJobs(client, apiUrl, accessToken, refreshTime, cancelToken);
  } catch (err) {
    console.error(`Device error (${accessToken}):`, err.message);
    if (client) client.end();
  }
}

export async function startGateway() {
  console.log("Starting gateway...");
  const config = await loadConfig();
  const { api_url, devices, jobRefreshTime } = config;

  devices.forEach((device) => {
    if (!activePollers.has(device.accessToken)) {
      const cancelToken = { active: true };
      processDevice(device, api_url, jobRefreshTime, cancelToken);
      activePollers.set(device.accessToken, cancelToken);
    }
  });
}

export async function reloadGateway() {
  console.log("Reloading gateway...");
  activePollers.forEach((token, key) => {
    token.active = false;
    activePollers.delete(key);
  });
  await startGateway();
}
