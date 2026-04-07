import { check, fail } from "k6";
import exec from "k6/execution";
import http from "k6/http";

import { login } from "../core/auth.js";
import {
  buildSensorRequests,
  createDevice,
  createDeviceTemplate,
  deleteDevice,
  deleteDeviceTemplate,
  replaceDeviceTemplateSensors,
} from "../core/devices.js";
import { buildUrl } from "../core/http.js";

const BASE_URL = "http://localhost:9001/api";
const USERNAME = "stress@gmail.com";
const PASSWORD = "stress";
const DEVICE_COUNT = 50;
const SENSORS_PER_DEVICE = 5;
const VUS = 100;
const DURATION = "30s";

export const options = {
  setupTimeout: "15m",
  teardownTimeout: "15m",
  scenarios: {
    http_datapoints: {
      executor: "constant-vus",
      vus: VUS,
      duration: DURATION,
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<1000"],
  },
};

export function setup() {
  validateConfig();

  const runId = `http-benchmark-${Date.now()}`;
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
        protocol: "HTTP",
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
  const deviceIndex = (exec.vu.idInTest - 1) % data.devices.length;
  const device = data.devices[deviceIndex];
  const now = Date.now();
  const payload = data.sensorTags.map((sensorTag, index) => ({
    tag: sensorTag,
    value: buildSensorValue(index),
    timeStamp: now,
  }));

  const response = http.post(
    buildUrl(BASE_URL, `/devices/${device.accessToken}/data`),
    JSON.stringify(payload),
    {
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      tags: { name: "CreateDataPoints" },
    }
  );

  check(response, {
    "datapoint write returned 204": (res) => res.status === 204,
  });
}

export function teardown(data) {
  cleanupResources(data.session, data.devices.map((device) => device.id), data.templateId);
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
    fail("K6_USERNAME and K6_PASSWORD must be set.");
  }

  if (!Number.isInteger(DEVICE_COUNT) || DEVICE_COUNT <= 0) {
    fail("DEVICE_COUNT must be a positive integer.");
  }

  if (!Number.isInteger(SENSORS_PER_DEVICE) || SENSORS_PER_DEVICE <= 0) {
    fail("SENSORS_PER_DEVICE must be a positive integer.");
  }
  if (!Number.isInteger(VUS) || VUS <= 0) {
    fail("VUS must be a positive integer.");
  }
}

function buildSensorValue(sensorIndex) {
  const iteration = exec.scenario.iterationInTest;
  const vuId = exec.vu.idInTest;

  return sensorIndex + iteration + vuId / 1000;
}
