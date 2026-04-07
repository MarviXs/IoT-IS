import { check, fail } from "k6";
import exec from "k6/execution";
import { Counter } from "k6/metrics";
import { Client } from "k6/x/mqtt";

import { createDataPointFlatBuffer } from "../core/flatbuffers.js";
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
const DEVICE_COUNT = 10;
const SENSORS_PER_DEVICE = 5;
const DURATION = "30s";
const CONNECT_TIMEOUT_MS = 2000;
const CONNECT_READY_TIMEOUT_MS = 5000;
const MQTT_QOS = 0;
const MQTT_RETAIN = false;
const PUBLISH_OPTIONS = Object.freeze({
  qos: MQTT_QOS,
  retain: MQTT_RETAIN,
});
const mqttPublishBatches = new Counter("mqtt_publish_batches");
const mqttDatapointsSent = new Counter("mqtt_datapoints_sent");
const mqttPublishFailures = new Counter("mqtt_publish_failures");

let batchSequence = 0;

export const options = {
  setupTimeout: "15m",
  teardownTimeout: "15m",
  scenarios: {
    mqtt_datapoints: {
      executor: "constant-vus",
      vus: DEVICE_COUNT,
      duration: DURATION,
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
        topic: `devices/${accessToken}/data`,
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
  const client = new Client({
    client_id: `k6-mqtt-${device.accessToken}-${exec.vu.idInTest}`,
    username: device.accessToken,
    password: "",
  });

  let isConnected = false;

  const connectTimeout = setTimeout(() => {
    if (isConnected) {
      return;
    }

    mqttPublishFailures.add(1);
    fail(`MQTT client did not connect for device ${device.accessToken}.`);
  }, CONNECT_READY_TIMEOUT_MS);

  client.on("connect", () => {
    isConnected = true;
    clearTimeout(connectTimeout);

    check(client, {
      "mqtt publisher is connected": (currentClient) => currentClient.connected,
    });

    publishBatch(client, device.topic, data.sensorTags);
  });

  client.on("end", () => {
    clearTimeout(connectTimeout);
  });

  client.connect(`mqtt://${MQTT_HOST}:${MQTT_PORT}`, {
    connect_timeout: CONNECT_TIMEOUT_MS,
    clean_session: false,
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
  const deviceIndex = (exec.vu.idInTest - 1) % devices.length;
  return devices[deviceIndex];
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
  const iteration = batchSequence;
  const vuId = exec.vu.idInTest;

  return sensorIndex + iteration + vuId / 1000;
}

function publishBatch(client, topic, sensorTags) {
  if (!client.connected || exec.scenario.progress >= 1) {
    endClient(client);
    return;
  }

  const timestamp = Date.now();
  let publishError;

  for (let sensorIndex = 0; sensorIndex < sensorTags.length; sensorIndex += 1) {
    const payload = createDataPointFlatBuffer(
      sensorTags[sensorIndex],
      buildSensorValue(sensorIndex),
      timestamp
    );

    try {
      client.publish(topic, payload, PUBLISH_OPTIONS);
      mqttDatapointsSent.add(1);
    } catch (error) {
      publishError = error;
      mqttPublishFailures.add(1);
      break;
    }
  }

  batchSequence += 1;

  check(publishError, {
    "mqtt datapoint publish succeeded": (error) => error === undefined,
  });

  if (publishError) {
    endClient(client);
    return;
  }

  mqttPublishBatches.add(1);
  setTimeout(() => {
    publishBatch(client, topic, sensorTags);
  }, 0);
}

function endClient(client) {
  try {
    client.end();
  } catch (_) {
  }
}
