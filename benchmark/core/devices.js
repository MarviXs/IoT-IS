import { authorizedJsonRequest } from "./auth.js";
import { formatError } from "./http.js";

export function createDeviceTemplate(session, templateRequest) {
  const result = authorizedJsonRequest(
    session,
    "POST",
    "/device-templates",
    templateRequest,
    {
      tags: { name: "CreateDeviceTemplate" },
    }
  );

  if (result.response.status !== 201 || !result.json) {
    throw new Error(formatError(result, "Device template creation failed"));
  }

  return result.json;
}

export function replaceDeviceTemplateSensors(
  session,
  templateId,
  sensorRequests
) {
  const result = authorizedJsonRequest(
    session,
    "PUT",
    `/device-templates/${templateId}/sensors`,
    sensorRequests,
    {
      tags: { name: "ReplaceDeviceTemplateSensors" },
    }
  );

  if (result.response.status !== 204) {
    throw new Error(formatError(result, "Updating template sensors failed"));
  }
}

export function createDevice(session, deviceRequest) {
  const result = authorizedJsonRequest(session, "POST", "/devices", deviceRequest, {
    tags: { name: "CreateDevice" },
  });

  if (result.response.status !== 201 || !result.json) {
    throw new Error(formatError(result, "Device creation failed"));
  }

  return result.json;
}

export function deleteDevice(session, deviceId) {
  const result = authorizedJsonRequest(
    session,
    "DELETE",
    `/devices/${deviceId}`,
    undefined,
    {
      tags: { name: "DeleteDevice" },
    }
  );

  if (result.response.status !== 204 && result.response.status !== 404) {
    throw new Error(formatError(result, `Deleting device ${deviceId} failed`));
  }
}

export function deleteDeviceTemplate(session, templateId) {
  const result = authorizedJsonRequest(
    session,
    "DELETE",
    `/device-templates/${templateId}`,
    undefined,
    {
      tags: { name: "DeleteDeviceTemplate" },
    }
  );

  if (result.response.status !== 204 && result.response.status !== 404) {
    throw new Error(
      formatError(result, `Deleting device template ${templateId} failed`)
    );
  }
}

export function buildSensorRequests(sensorCount) {
  return Array.from({ length: sensorCount }, (_, index) => ({
    id: null,
    tag: `sensor_${index + 1}`,
    name: `Sensor ${index + 1}`,
    unit: "unit",
    accuracyDecimals: 2,
    group: null,
  }));
}
