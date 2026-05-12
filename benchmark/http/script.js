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
import { createScene, deleteScene } from "../core/scenes.js";

const BASE_URL = "http://localhost:9001/api";
const USERNAME = "stress@gmail.com";
const PASSWORD = "stress";
const DEVICE_COUNT = 1;
const SENSORS_PER_DEVICE = 5;
const CREATE_SCENES_WITH_DEVICES = true;
const CREATE_SCENE_NOTIFICATIONS = false;
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
  const createdSceneIds = [];
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

    if (CREATE_SCENES_WITH_DEVICES) {
      for (const [index, device] of devices.entries()) {
        const sceneId = createScene(
          session,
          buildComplexSceneRequest(runId, index + 1, device.id, sensorRequests)
        );
        createdSceneIds.push(sceneId);
      }
    }

    return {
      session,
      templateId,
      devices,
      sceneIds: createdSceneIds,
      sensorTags: sensorRequests.map((sensor) => sensor.tag),
    };
  } catch (error) {
    cleanupResources(session, createdSceneIds, createdDeviceIds, templateId);
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
  cleanupResources(
    data.session,
    data.sceneIds || [],
    data.devices.map((device) => device.id),
    data.templateId
  );
}

function cleanupResources(session, sceneIds, deviceIds, templateId) {
  for (const sceneId of sceneIds) {
    try {
      deleteScene(session, sceneId);
    } catch (error) {
      console.error(`Cleanup failed for scene ${sceneId}: ${error.message}`);
    }
  }

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
  if (typeof CREATE_SCENES_WITH_DEVICES !== "boolean") {
    fail("CREATE_SCENES_WITH_DEVICES must be a boolean.");
  }
  if (typeof CREATE_SCENE_NOTIFICATIONS !== "boolean") {
    fail("CREATE_SCENE_NOTIFICATIONS must be a boolean.");
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

function buildComplexSceneRequest(runId, sceneNumber, deviceId, sensors) {
  return {
    name: `${runId}-scene-${sceneNumber}`,
    description: `Nested benchmark scene for device ${sceneNumber}`,
    isEnabled: true,
    condition: JSON.stringify(buildNestedCondition(deviceId, sensors)),
    actions: CREATE_SCENE_NOTIFICATIONS
      ? [
          {
            type: "NOTIFICATION",
            deviceId: null,
            recipeId: null,
            notificationSeverity: "Info",
            notificationMessage: `Benchmark scene ${sceneNumber} triggered`,
            discordWebhookUrl: null,
            includeSensorValues: false,
          },
        ]
      : [],
    cooldownAfterTriggerTime: 0,
  };
}

function buildNestedCondition(deviceId, sensors) {
  const comparisons = sensors.map((sensor, index) => {
    const sensorVar = { var: `device.${deviceId}.${sensor.tag}` };
    const operators = [
      [">", -1000000],
      ["<", 1000000],
      [">=", -1000000],
      ["!=", -1000000],
      ["<=", 1000000],
    ];
    const [operator, value] = operators[index % operators.length];

    return { [operator]: [sensorVar, value] };
  });

  if (comparisons.length === 1) {
    return comparisons[0];
  }

  if (comparisons.length === 2) {
    return {
      and: [
        comparisons[0],
        {
          or: [comparisons[1]],
        },
      ],
    };
  }

  return {
    and: [
      comparisons[0],
      {
        or: [
          comparisons[1],
          {
            and: comparisons.slice(2),
          },
        ],
      },
    ],
  };
}
