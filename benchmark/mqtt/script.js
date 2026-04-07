import { check, fail, sleep } from "k6";
import exec from "k6/execution";
import { Client } from "k6/x/mqtt";

import {
  createDataPointFlatBufferFromBytes,
  encodeAsciiString,
} from "../core/flatbuffers.js";
import { login } from "../core/auth.js";
import {
  buildSensorRequests,
  createDevice,
  createDeviceTemplate,
  deleteDevice,
  deleteDeviceTemplate,
  replaceDeviceTemplateSensors,
} from "../core/devices.js";

const BASE_URL = "http://localhost:9001/api";
const MQTT_HOST = "localhost";
const MQTT_PORT = "1883";
const USERNAME = "stress@gmail.com";
const PASSWORD = "stress";
const DEVICE_COUNT = 100;
const SENSORS_PER_DEVICE = 5;
const DURATION = "30s";
const CONNECT_TIMEOUT_MS = 2000;
const CONNECT_READY_TIMEOUT_MS = 5000;
const CONNECT_READY_POLL_SECONDS = 0.01;
const MQTT_QOS = 0;
const MQTT_RETAIN = false;

let cachedPublisher;
let cachedDeviceAccessToken;
let cachedTopics;
let cachedSensorTagBytes;

export const options = {
  setupTimeout: "15m",
  teardownTimeout: "15m",
  scenarios: {
    mqtt_datapoints: {
      executor: "constant-vus",
      vus: DEVICE_COUNT,
      duration: DURATION,
      gracefulStop: "0s",
    },
  },
  thresholds: {
    checks: ["rate>0.99"],
  },
};

export function setup() {
  validateConfig();

  const runId = `mqtt-benchmark-${Date.now()}`;
  const session = login(BASE_URL, USERNAME, PASSWORD);
  const sensorRequests = buildSensorRequests(SENSORS_PER_DEVICE);
  const createdDeviceIds = [];
  let templateId = null;

  try {
    templateId = createDeviceTemplate(session, {
      name: `${runId}-template`,
      deviceType: "Generic",
    });

    replaceDeviceTemplateSensors(session, templateId, sensorRequests);

    const devices = Array.from({ length: DEVICE_COUNT }, (_, index) => {
      const deviceNumber = index + 1;
      const accessToken = `${runId}-device-${deviceNumber}`;
      const deviceId = createDevice(session, {
        name: `${runId}-device-${deviceNumber}`,
        accessToken,
        templateId,
        protocol: "MQTT",
        dataPointRetentionDays: 30,
        sampleRateSeconds: 1,
      });

      createdDeviceIds.push(deviceId);

      return {
        id: deviceId,
        accessToken,
      };
    });

    return {
      session,
      templateId,
      devices,
      sensorTags: sensorRequests.map((sensor) => sensor.tag),
    };
  } catch (error) {
    cleanupResources(session, createdDeviceIds, templateId);
    throw error;
  }
}

export default function (data) {
  const device = getAssignedDevice(data.devices);
  const client = getPublisher(device, data.sensorTags);

  check(client, {
    "mqtt publisher is connected": (currentClient) => currentClient.connected,
  });

  const timestamp = Date.now();
  let publishError;
  for (let sensorIndex = 0; sensorIndex < data.sensorTags.length; sensorIndex += 1) {
    const payload = createDataPointFlatBufferFromBytes(
      cachedSensorTagBytes[sensorIndex],
      buildSensorValue(sensorIndex),
      timestamp
    );

    try {
      client.publish(
        cachedTopics[sensorIndex],
        payload,
        {
          qos: MQTT_QOS,
          retain: MQTT_RETAIN,
        }
      );
    } catch (error) {
      publishError = error;
      break;
    }
  }

  check(publishError, {
    "mqtt datapoint publish succeeded": (error) => error === undefined,
  });
}

export function teardown(data) {
  cleanupResources(
    data.session,
    data.devices.map((device) => device.id),
    data.templateId
  );
}

function getAssignedDevice(devices) {
  const deviceIndex = exec.vu.idInTest - 1;
  return devices[deviceIndex];
}

function getPublisher(device, sensorTags) {
  if (cachedPublisher && cachedDeviceAccessToken === device.accessToken) {
    return cachedPublisher;
  }

  if (cachedPublisher) {
    try {
      cachedPublisher.end();
    } catch (_) {
    }
  }

  cachedTopics = sensorTags.map(() => `devices/${device.accessToken}/data`);
  cachedSensorTagBytes = sensorTags.map((tag) => encodeAsciiString(tag));

  cachedPublisher = createPublisher(device.accessToken);
  cachedDeviceAccessToken = device.accessToken;

  return cachedPublisher;
}

function createPublisher(deviceAccessToken) {
  const clientId = `k6-mqtt-${deviceAccessToken}-${exec.vu.idInTest}`;
  const publisher = new Client({
    client_id: clientId,
    username: deviceAccessToken,
    password: "",
  });

  publisher.connect(`mqtt://${MQTT_HOST}:${MQTT_PORT}`, {
    connect_timeout: CONNECT_TIMEOUT_MS,
    clean_session: false,
  });

  const connectDeadline = Date.now() + CONNECT_READY_TIMEOUT_MS;
  while (!publisher.connected && Date.now() < connectDeadline) {
    sleep(CONNECT_READY_POLL_SECONDS);
  }

  if (!publisher.connected) {
    fail(`MQTT client did not connect for device ${deviceAccessToken}.`);
  }

  return publisher;
}

function cleanupResources(session, deviceIds, templateId) {
  for (const deviceId of deviceIds) {
    try {
      deleteDevice(session, deviceId);
    } catch (error) {
      console.error(`Cleanup failed for device ${deviceId}: ${error.message}`);
    }
  }

  if (!templateId) {
    return;
  }

  try {
    deleteDeviceTemplate(session, templateId);
  } catch (error) {
    console.error(
      `Cleanup failed for device template ${templateId}: ${error.message}`
    );
  }
}

function validateConfig() {
  if (!USERNAME || !PASSWORD) {
    fail("USERNAME and PASSWORD must be set.");
  }

  if (!MQTT_HOST || !MQTT_PORT) {
    fail("MQTT_HOST and MQTT_PORT must be set.");
  }

  if (!Number.isInteger(DEVICE_COUNT) || DEVICE_COUNT <= 0) {
    fail("DEVICE_COUNT must be a positive integer.");
  }

  if (!Number.isInteger(SENSORS_PER_DEVICE) || SENSORS_PER_DEVICE <= 0) {
    fail("SENSORS_PER_DEVICE must be a positive integer.");
  }
}

function buildSensorValue(sensorIndex) {
  const iteration = exec.scenario.iterationInTest;
  const vuId = exec.vu.idInTest;

  return sensorIndex + iteration + vuId / 1000;
}
