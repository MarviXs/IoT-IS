import net from "net";
import { parseStringPromise } from "xml2js";

// Create a TCP connection to the device
export function createDeviceConnection(address) {
  return new Promise((resolve, reject) => {
    const [host, port] = address.split(":");
    const client = new net.Socket();
    const onError = (err) => reject(err);
    client.once("error", onError);
    client.setMaxListeners(30);

    client.connect(parseInt(port, 10), host, () => resolve(client));
  });
}

export function calculateChecksum(buffer) {
  let sum = 0;
  for (const byte of buffer) {
    sum = (sum + byte) & 0xff;
  }
  return (~sum + 1) & 0xff;
}

export function buildPacket(devAdr, cmd, payload) {
  const dataPart = Buffer.concat([Buffer.from([devAdr, cmd]), payload]);
  const length = dataPart.length + 1;
  const nextB = Buffer.alloc(3);
  nextB.writeUIntBE(length, 0, 3);
  const packetWithoutChecksum = Buffer.concat([nextB, dataPart]);
  const checksum = calculateChecksum(packetWithoutChecksum);
  return Buffer.concat([packetWithoutChecksum, Buffer.from([checksum])]);
}

export function readPacket(client) {
  return new Promise((resolve, reject) => {
    let buffer = Buffer.alloc(0);

    // Cleanup function to remove listeners
    function cleanup() {
      client.removeListener("data", onData);
      client.removeListener("error", onError);
    }

    // Handle incoming data
    function onData(chunk) {
      buffer = Buffer.concat([buffer, chunk]);
      if (buffer.length >= 3) {
        const expectedLength = 3 + buffer.readUIntBE(0, 3);
        if (buffer.length >= expectedLength) {
          cleanup(); // Remove listeners when the packet is fully received
          const packet = buffer.slice(0, expectedLength);
          resolve(packet);
        }
      }
    }

    // Handle errors
    function onError(err) {
      cleanup(); // Remove listeners on error
      reject(err);
    }

    client.on("data", onData);
    client.on("error", onError);
  });
}

export function parsePacket(buffer) {
  if (buffer.length < 5) throw new Error("Packet too short.");
  const nextB = buffer.readUIntBE(0, 3);
  if (buffer.length !== 3 + nextB) throw new Error("Invalid packet length.");
  const devAdr = buffer[3];
  const cmd = buffer[4];
  const payload = buffer.slice(5, buffer.length - 1);
  const checksum = buffer[buffer.length - 1];
  const calculated = calculateChecksum(buffer.slice(0, buffer.length - 1));
  if (checksum !== calculated) throw new Error("Checksum mismatch.");
  return { nextB, devAdr, cmd, payload, checksum };
}

export async function sendCommand(client, packet) {
  return new Promise((resolve, reject) => {
    client.write(packet, (err) => {
      if (err) return reject(err);
      readPacket(client)
        .then((rawPacket) => {
          try {
            const parsed = parsePacket(rawPacket);
            resolve(parsed);
          } catch (error) {
            client.removeListener("data", onData);
            reject(error);
          }
        })
        .catch(reject);
    });
  });
}

export function extractMeasurements(packetOrPayload) {
  const payload =
    packetOrPayload.payload !== undefined
      ? packetOrPayload.payload
      : packetOrPayload.slice(5, packetOrPayload.length - 1);
  if (payload.length < 241)
    throw new Error("Measurement payload is too short.");

  const trustedROI = payload.readUIntBE(3, 1);

  const extract = (start) => ({
    currentCPS: payload.readFloatLE(start),
    currentDP: payload.readFloatLE(start + 4),
    cps: payload.readFloatLE(start + 12),
    dp: payload.readFloatLE(start + 16),
  });

  const starts = [64, 111, 158, 205];
  const start = starts[trustedROI];
  const extracted = extract(start);

  return extracted;
}

export async function getDetectorData(client) {
  const packet = buildPacket(0x00, 0xbe, Buffer.from([0x00]));
  return await sendCommand(client, packet);
}

export async function getMeasurements(client) {
  const detectorData = await getDetectorData(client);
  const measurements = extractMeasurements(detectorData);
  return measurements;
}

export async function linkDevice(client) {
  const packet = buildPacket(0x00, 0x01, Buffer.alloc(0));
  const parsedResponse = await sendCommand(client, packet);
  const firmware = parsedResponse.payload.toString("utf8");
  console.log("Firmware:", firmware);
  return firmware;
}

export async function ensureClientAlive(client) {
  if (!client || client.destroyed) {
    console.log(`Reconnecting to device ${client?.accessToken ?? "unknown"}`);
    if (!client?.deviceAddress) {
      throw new Error("Device address unavailable.");
    }
    const newClient = await createDeviceConnection(client.deviceAddress);
    newClient.deviceAddress = client.deviceAddress;
    newClient.accessToken = client.accessToken;
    return newClient;
  }
  return client;
}

export async function getDeviceDescription(client, languageCode = 1) {
  if (languageCode < 1 || languageCode > 255) {
    throw new Error("Language code must be in range 1-255.");
  }

  // Payload: 1 byte language code, followed by 5 bytes of zeros
  const payload = Buffer.from([languageCode, 0x00, 0x00, 0x00, 0x00, 0x00]);

  // Build packet using command 0xD2
  const packet = buildPacket(0x00, 0xd2, payload);

  // Send command and get response
  const response = await sendCommand(client, packet);

  // Convert ASCII payload to string
  const xmlString = response.payload.toString("ascii");

  // Parse XML to JSON using xml2js
  try {
    const result = await parseStringPromise(xmlString, {
      explicitArray: false,
    });
    return result;
  } catch (err) {
    throw new Error(`Failed to parse XML: ${err.message}`);
  }
}

export async function getDeviceParameters(client, paramIds = null) {
  // If no parameter IDs are provided, return nothing (null)
  if (!paramIds || paramIds.length === 0) {
    return null;
  }

  // Convert array of parameter IDs to a comma-separated ASCII string, ending with null byte
  const idsString = paramIds.join(",") + "\x00";
  const payload = Buffer.from(idsString, "ascii");

  // Command code 0xC2 (194)
  const packet = buildPacket(0x00, 0xc2, payload);

  // Send and receive response
  const response = await sendCommand(client, packet);

  // Convert and clean payload to string
  const responseStr = response.payload.toString("ascii").replace(/\x00.*$/, ""); // Remove null terminator and any trailing garbage

  // Parse into { ID: value } object
  const paramMap = {};
  responseStr.split(",").forEach((entry) => {
    const [id, value] = entry.split("=");
    if (id && value !== undefined) {
      paramMap[id.trim()] = value.trim();
    }
  });

  return paramMap;
}

export async function setDeviceParameter(client, paramId, value) {
  if (!paramId || typeof value === "undefined") {
    throw new Error("Parameter ID and value must be provided.");
  }

  // Create the payload string "ID=Hodnota\0"
  const payloadStr = `${paramId}=${value}\x00`;
  const payload = Buffer.from(payloadStr, "ascii");

  // Command code for setting a parameter: 0xC3 (195)
  const packet = buildPacket(0x00, 0xc3, payload);

  // Send the packet and wait for the response
  const response = await sendCommand(client, packet);

  // Convert response payload to ASCII and clean up
  const responseStr = response.payload.toString("ascii").replace(/\x00.*$/, ""); // remove null and trailing

  // Parse and return the confirmed param and its actual value
  const [returnedId, returnedValue] = responseStr.split("=");
  if (!returnedId || typeof returnedValue === "undefined") {
    throw new Error("Invalid response format.");
  }

  return {
    id: returnedId.trim(),
    value: returnedValue.trim(),
  };
}

export async function getTotalNumberOfChannels(client) {
  const result = await getDeviceParameters(client, [110]);
  const numberOfChannels = parseInt(result["110"], 10);
  if (isNaN(numberOfChannels)) {
    throw new Error("Failed to retrieve the number of channels.");
  }
  return numberOfChannels;
}

export async function setChannelIndex(client, channelIndex) {
  if (channelIndex < 0 || channelIndex > 255) {
    throw new Error("Channel index must be in range 0-255.");
  }

  const result = await setDeviceParameter(client, 111, channelIndex);

  if (result.id !== "111") {
    throw new Error(
      `Failed to set channel index. Expected ID 111, got ${result.id}`
    );
  }
}
