import { check, fail } from "k6";
import exec from "k6/execution";
import { Counter, Trend } from "k6/metrics";
import { Client } from "k6/x/mqtt";

import { createDataPointFlatBuffer } from "../core/flatbuffers.js";
import { login } from "../core/auth.js";
import {
  buildSensorRequests,
  createDevice,
  createDeviceTemplate,
  deleteDevice,
  deleteDeviceTemplate,
  replaceDeviceTemplateSensors
} from "../core/devices.js";

const BASE_URL = "http://localhost:9001/api";
const MQTT_HOST = "localhost";
const MQTT_PORT = "1883";
const USERNAME = "stress@gmail.com";
const PASSWORD = "stress";
const DEVICE_COUNT = 1;
const SENSORS_PER_DEVICE = 1;
const DURATION = "10s";
const CONNECT_TIMEOUT_MS = 2000;
const CONNECT_READY_TIMEOUT_MS = 5000;
const MQTT_QOS = 1;
const MQTT_RETAIN = false;
const MQTT_KEEPALIVE_SECONDS = 60;
const ENQUEUE_BATCH_SIZE = 64;
const ENQUEUE_INTERVAL_MS = 0;
const MAX_PENDING_PUBLISHES = 32;
const DRAIN_TIMEOUT_MS = 5000;
const PUBLISH_OPTIONS = Object.freeze({
  qos: MQTT_QOS,
  retain: MQTT_RETAIN,
});
const mqttPublishBatches = new Counter("mqtt_publish_batches");
const mqttDatapointsSent = new Counter("mqtt_datapoints_sent");
const mqttDatapointsAcked = new Counter("mqtt_datapoints_acked");
const mqttPublishFailures = new Counter("mqtt_publish_failures");
const mqttPublishLatency = new Trend("mqtt_publish_latency", true);

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
    mqtt_publish_latency: ["p(95)<1000"],
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
    client_id: device.accessToken,
    username: device.accessToken,
    password: "",
  });

  const state = {
    isConnected: false,
    isEnding: false,
    isEnded: false,
    pendingPublishes: 0,
    sequence: 0,
    sensorIndex: 0,
    drainDeadline: 0,
  };

  const connectTimeout = setTimeout(() => {
    if (state.isConnected) {
      return;
    }

    mqttPublishFailures.add(1);
    fail(`MQTT client did not connect for device ${device.accessToken}.`);
  }, CONNECT_READY_TIMEOUT_MS);

  client.on("connect", () => {
    state.isConnected = true;
    clearTimeout(connectTimeout);

    check(client, {
      "mqtt publisher is connected": (currentClient) => currentClient.connected,
    });

    pumpPublishQueue(client, device.topic, data.sensorTags, state);
  });

  client.on("end", () => {
    clearTimeout(connectTimeout);
    state.isConnected = false;
    state.isEnding = true;
  });

  client.on("error", () => {
    clearTimeout(connectTimeout);
    state.isConnected = false;
    state.isEnding = true;
  });

  client.connect(`mqtt://${MQTT_HOST}:${MQTT_PORT}`, {
    connect_timeout: CONNECT_TIMEOUT_MS,
    clean_session: true,
    keepalive: MQTT_KEEPALIVE_SECONDS,
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

  if (!Number.isInteger(MQTT_QOS) || MQTT_QOS < 0 || MQTT_QOS > 2) {
    fail("MQTT_QOS must be 0, 1, or 2.");
  }

  if (!Number.isInteger(ENQUEUE_BATCH_SIZE) || ENQUEUE_BATCH_SIZE <= 0) {
    fail("ENQUEUE_BATCH_SIZE must be a positive integer.");
  }

  if (!Number.isInteger(MAX_PENDING_PUBLISHES) || MAX_PENDING_PUBLISHES <= 0) {
    fail("MAX_PENDING_PUBLISHES must be a positive integer.");
  }
}

function buildSensorValue(sensorIndex, sequence) {
  const vuId = exec.vu.idInTest;

  return sensorIndex + sequence + vuId / 1000;
}

function pumpPublishQueue(client, topic, sensorTags, state) {
  if (!client.connected) {
    state.isConnected = false;
    endClient(client, state);
    return;
  }

  if (state.isEnding || state.isEnded) {
    return;
  }

  if (exec.scenario.progress >= 1) {
    drainAndEndClient(client, state);
    return;
  }

  let enqueued = 0;
  while (enqueued < ENQUEUE_BATCH_SIZE && state.pendingPublishes < MAX_PENDING_PUBLISHES) {
    enqueuePublish(client, topic, sensorTags, state);
    enqueued += 1;
  }

  if (enqueued > 0) {
    mqttPublishBatches.add(1);
  }

  const nextInterval = enqueued === 0 ? Math.max(ENQUEUE_INTERVAL_MS, 1) : ENQUEUE_INTERVAL_MS;
  setTimeout(() => {
    pumpPublishQueue(client, topic, sensorTags, state);
  }, nextInterval);
}

function enqueuePublish(client, topic, sensorTags, state) {
  const sensorIndex = state.sensorIndex;
  const payload = createDataPointFlatBuffer(
    sensorTags[sensorIndex],
    buildSensorValue(sensorIndex, state.sequence),
    Date.now()
  );

  state.sensorIndex = (state.sensorIndex + 1) % sensorTags.length;
  if (state.sensorIndex === 0) {
    state.sequence += 1;
  }

  try {
    state.pendingPublishes += 1;
    mqttDatapointsSent.add(1);
    const publishStartedAt = Date.now();
    client.publishAsync(topic, payload, PUBLISH_OPTIONS).then(
      () => {
        state.pendingPublishes -= 1;
        mqttPublishLatency.add(Date.now() - publishStartedAt);
        mqttDatapointsAcked.add(1);
        endClientWhenDrained(client, state);
      },
      () => {
        state.pendingPublishes -= 1;
        mqttPublishLatency.add(Date.now() - publishStartedAt);
        mqttPublishFailures.add(1);
        state.isConnected = false;
        state.isEnding = true;
        endClientWhenDrained(client, state);
      }
    );
  } catch (_) {
    state.pendingPublishes -= 1;
    mqttPublishFailures.add(1);
    state.isConnected = false;
    state.isEnding = true;
    endClientWhenDrained(client, state);
  }
}

function drainAndEndClient(client, state) {
  if (state.isEnding) {
    endClientWhenDrained(client, state);
    return;
  }

  state.isEnding = true;
  state.drainDeadline = Date.now() + DRAIN_TIMEOUT_MS;
  endClientWhenDrained(client, state);
}

function endClientWhenDrained(client, state) {
  if (!state.isEnding) {
    return;
  }

  if (state.pendingPublishes <= 0 || Date.now() >= state.drainDeadline) {
    endClient(client, state);
    return;
  }

  setTimeout(() => {
    endClientWhenDrained(client, state);
  }, 10);
}

function endClient(client, state) {
  if (state && state.isEnded) {
    return;
  }

  if (state) {
    state.isEnding = true;
    state.isEnded = true;
  }

  try {
    client.end();
  } catch (_) {
  }
}
