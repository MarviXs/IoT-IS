import net from 'net';

const DEVICE_ADDRESS = 0x01;

function computeChecksum(buffer) {
  let sum = 0;
  for (let i = 0; i < buffer.length - 1; i++) {
    sum = (sum + buffer[i]) & 0xFF;
  }
  return ((~sum) + 1) & 0xFF;
}

function createResponsePacket(cmd, dataBuffer) {
  // Packet structure: 3 bytes length, 1 byte devAdr, 1 byte cmd, data, 1 byte checksum.
  const packetBodyLength = 1 + 1 + dataBuffer.length + 1; // devAdr + cmd + data + checksum
  const totalLength = 3 + packetBodyLength;
  const packet = Buffer.alloc(totalLength);

  packet[0] = (packetBodyLength >> 16) & 0xFF;
  packet[1] = (packetBodyLength >> 8) & 0xFF;
  packet[2] = packetBodyLength & 0xFF;

  packet[3] = DEVICE_ADDRESS;
  packet[4] = cmd;

  dataBuffer.copy(packet, 5);

  packet[totalLength - 1] = computeChecksum(packet);

  return packet;
}

// Create data part of response for command cmGetDetector (0xBE) with predefined values.
function createCmGetDetectorData() {
  const data = Buffer.alloc(241);
  let offset = 0;
  // 1st byte: Reserved
  data.writeUInt8(0, offset); offset++;
  // 2nd byte: Detector type (e.g. 1 = SCA4)
  data.writeUInt8(1, offset); offset++;
  // 3rd byte: Short MStamp (0)
  data.writeUInt8(0, offset); offset++;
  // 4th byte: TrustedROI (0 = normal, 255 = overloaded)
  data.writeUInt8(0, offset); offset++;
  // 5th-6th byte: Reserved
  data.writeUInt8(0, offset); offset++;
  data.writeUInt8(0, offset); offset++;
  // 7th byte: HV status (1 = ON)
  data.writeUInt8(1, offset); offset++;
  // 8th-9th byte: Preset HV value, unsigned short (230)
  data.writeUInt16LE(230, offset); offset += 2;
  // 10th-11th byte: temperature (signed short), 250 => 25.0°C
  data.writeInt16LE(250, offset); offset += 2;
  // 12th-30th byte: GeoStart (19 bytes)
  data.writeUInt8(1, offset); offset++; // GPS valid
  data.writeUInt32LE(123456789, offset); offset += 4;
  data.writeUInt32LE(987654321, offset); offset += 4;
  data.writeUInt16LE(100, offset); offset += 2;
  data.writeUInt32LE(3600, offset); offset += 4;
  data.writeUInt32LE(20230325, offset); offset += 4;
  // 31st-49th byte: GeoEnd (19 bytes)
  data.writeUInt8(1, offset); offset++;
  data.writeUInt32LE(123456790, offset); offset += 4;
  data.writeUInt32LE(987654322, offset); offset += 4;
  data.writeUInt16LE(101, offset); offset += 2;
  data.writeUInt32LE(3601, offset); offset += 4;
  data.writeUInt32LE(20230326, offset); offset += 4;
  // 50th-53rd byte: MeasStamp (unsigned long, 1)
  data.writeUInt32LE(1, offset); offset += 4;
  // 54th-55th byte: HV1 status (unsigned short, 1)
  data.writeUInt16LE(1, offset); offset += 2;
  // 56th byte: MeasRunning1 (1)
  data.writeUInt8(1, offset); offset++;
  // 57th-60th byte: CurrentMeasTime1 (unsigned long, 1000)
  data.writeUInt32LE(1000, offset); offset += 4;
  // 61st-64th byte: CurrentCount1 (unsigned long, 100)
  data.writeUInt32LE(100, offset); offset += 4;
  // 65th-68th byte: CurrentCPS1 (float, 10.0)
  data.writeFloatLE(10.0, offset); offset += 4;
  // 69th-72nd byte: CurrentDP1 (float, 0.5)
  data.writeFloatLE(0.5, offset); offset += 4;
  // 73rd-76th byte: Count1 (unsigned long, 500)
  data.writeUInt32LE(500, offset); offset += 4;
  // 77th-80th byte: CPS1 (float, 9.5)
  data.writeFloatLE(9.5, offset); offset += 4;
  // 81st-84th byte: DP1 (float, 0.45)
  data.writeFloatLE(0.45, offset); offset += 4;
  // 85th-88th byte: CPS1dev (float, 2.0)
  data.writeFloatLE(2.0, offset); offset += 4;
  // 89th-92nd byte: TrmCPS1 (float, 8.0)
  data.writeFloatLE(8.0, offset); offset += 4;
  // 93rd-96th byte: ShrtTrmCPS1dev (float, 1.5)
  data.writeFloatLE(1.5, offset); offset += 4;
  // 97th-100th byte: TrustTime1 (float, 30.0)
  data.writeFloatLE(30.0, offset); offset += 4;
  // 101st-102nd byte: HV2 status (unsigned short, 1)
  data.writeUInt16LE(1, offset); offset += 2;
  // 103rd byte: MeasRunning2 (1)
  data.writeUInt8(1, offset); offset++;
  // 104th-107th byte: CurrentMeasTime2 (unsigned long, 1200)
  data.writeUInt32LE(1200, offset); offset += 4;
  // 108th-111th byte: CurrentCount2 (unsigned long, 200)
  data.writeUInt32LE(200, offset); offset += 4;
  // 112th-115th byte: CurrentCPS2 (float, 11.0)
  data.writeFloatLE(11.0, offset); offset += 4;
  // 116th-119th byte: CurrentDP2 (float, 0.55)
  data.writeFloatLE(0.55, offset); offset += 4;
  // 120th-123rd byte: Count2 (unsigned long, 600)
  data.writeUInt32LE(600, offset); offset += 4;
  // 124th-127th byte: CPS2 (float, 10.5)
  data.writeFloatLE(10.5, offset); offset += 4;
  // 128th-131st byte: DP2 (float, 0.50)
  data.writeFloatLE(0.50, offset); offset += 4;
  // 132nd-135th byte: CPS2dev (float, 2.5)
  data.writeFloatLE(2.5, offset); offset += 4;
  // 136th-139th byte: TrmCPS2 (float, 8.5)
  data.writeFloatLE(8.5, offset); offset += 4;
  // 140th-143rd byte: ShrtTrmCPS2dev (float, 1.8)
  data.writeFloatLE(1.8, offset); offset += 4;
  // 144th-147th byte: TrustTime2 (float, 40.0)
  data.writeFloatLE(40.0, offset); offset += 4;
  // 148th-149th byte: HV3 status (unsigned short, 1)
  data.writeUInt16LE(1, offset); offset += 2;
  // 150th byte: MeasRunning3 (1)
  data.writeUInt8(1, offset); offset++;
  // 151st-154th byte: CurrentMeasTime3 (unsigned long, 1500)
  data.writeUInt32LE(1500, offset); offset += 4;
  // 155th-158th byte: CurrentCount3 (unsigned long, 300)
  data.writeUInt32LE(300, offset); offset += 4;
  // 159th-162nd byte: CurrentCPS3 (float, 12.0)
  data.writeFloatLE(12.0, offset); offset += 4;
  // 163rd-166th byte: CurrentDP3 (float, 0.65)
  data.writeFloatLE(0.65, offset); offset += 4;
  // 167th-170th byte: Count3 (unsigned long, 700)
  data.writeUInt32LE(700, offset); offset += 4;
  // 171st-174th byte: CPS3 (float, 11.5)
  data.writeFloatLE(11.5, offset); offset += 4;
  // 175th-178th byte: DP3 (float, 0.60)
  data.writeFloatLE(0.60, offset); offset += 4;
  // 179th-182nd byte: CPS3dev (float, 3.0)
  data.writeFloatLE(3.0, offset); offset += 4;
  // 183rd-186th byte: TrmCPS3 (float, 9.0)
  data.writeFloatLE(9.0, offset); offset += 4;
  // 187th-190th byte: ShrtTrmCPS3dev (float, 2.2)
  data.writeFloatLE(2.2, offset); offset += 4;
  // 191st-194th byte: TrustTime3 (float, 50.0)
  data.writeFloatLE(50.0, offset); offset += 4;
  // 195th-196th byte: HV4 status (unsigned short, 1)
  data.writeUInt16LE(1, offset); offset += 2;
  // 197th byte: MeasRunning4 (1)
  data.writeUInt8(1, offset); offset++;
  // 198th-201st byte: CurrentMeasTime4 (unsigned long, 1800)
  data.writeUInt32LE(1800, offset); offset += 4;
  // 202nd-205th byte: CurrentCount4 (unsigned long, 400)
  data.writeUInt32LE(400, offset); offset += 4;
  // 206th-209th byte: CurrentCPS4 (float, 13.0)
  data.writeFloatLE(13.0, offset); offset += 4;
  // 210th-213th byte: CurrentDP4 (float, 0.70)
  data.writeFloatLE(0.70, offset); offset += 4;
  // 214th-217th byte: Count4 (unsigned long, 800)
  data.writeUInt32LE(800, offset); offset += 4;
  // 218th-221st byte: CPS4 (float, 12.5)
  data.writeFloatLE(12.5, offset); offset += 4;
  // 222nd-225th byte: DP4 (float, 0.68)
  data.writeFloatLE(0.68, offset); offset += 4;
  // 226th-229th byte: CPS4dev (float, 3.5)
  data.writeFloatLE(3.5, offset); offset += 4;
  // 230th-233rd byte: TrmCPS4 (float, 10.0)
  data.writeFloatLE(10.0, offset); offset += 4;
  // 234th-237th byte: ShrtTrmCPS4dev (float, 2.5)
  data.writeFloatLE(2.5, offset); offset += 4;
  // 238th-241st byte: TrustTime4 (float, 60.0)
  data.writeFloatLE(60.0, offset); offset += 4;

  return data;
}

// Create data part of response for cmLink (0x01)
// Returns firmware version as a string
function createCmLinkData() {
  const firmwareStr = "MSU V1.1.8";
  return Buffer.from(firmwareStr, 'utf8');
}

// Create TCP server on port 3001
const server = net.createServer((socket) => {
  console.log('Client connected:', socket.remoteAddress, socket.remotePort);
  let recvBuffer = Buffer.alloc(0);

  socket.on('data', (chunk) => {
    // Accumulate data
    recvBuffer = Buffer.concat([recvBuffer, chunk]);

    // Process complete packets in the buffer
    while (recvBuffer.length >= 3) {
      // The first 3 bytes contain the length of the following bytes
      const bodyLength = (recvBuffer[0] << 16) | (recvBuffer[1] << 8) | recvBuffer[2];
      const packetLength = 3 + bodyLength; // total packet length

      if (recvBuffer.length < packetLength) {
        // If we don't yet have a complete packet, wait for more data
        break;
      }

      // Extract the entire packet
      const packet = recvBuffer.slice(0, packetLength);
      // Remove the processed packet from the buffer
      recvBuffer = recvBuffer.slice(packetLength);

      // Verify checksum
      const receivedChecksum = packet[packet.length - 1];
      const calculatedChecksum = computeChecksum(packet);
      if (receivedChecksum !== calculatedChecksum) {
        console.error('Invalid checksum! Received:', receivedChecksum, 'Calculated:', calculatedChecksum);
        // In case of checksum error, ignore the packet
        continue;
      }

      // Get the command (Cmd) – ignore DevAdr in the incoming packet, as we always respond as DEVICE_ADDRESS (0x01)
      const cmd = packet[4];
      const dataPart = packet.slice(5, packet.length - 1);

      console.log(`Received command 0x${cmd.toString(16)} (data length: ${dataPart.length} bytes)`);

      if (cmd === 0xBE) { // cmGetDetector
        // Data part is expected to be 1 byte (reserved)
        if (dataPart.length !== 1) {
          console.error('Incorrect data length for cmGetDetector');
          continue;
        }
        // Create data part with predefined values
        const detectorData = createCmGetDetectorData();
        const response = createResponsePacket(cmd, detectorData);
        socket.write(response);
        console.log('Sent response for cmGetDetector');
      } else if (cmd === 0x01) { // cmLink
        // For cmLink, data is expected to be absent
        if (dataPart.length !== 0) {
          console.error('Incorrect data length for cmLink');
          continue;
        }
        const linkData = createCmLinkData();
        const response = createResponsePacket(cmd, linkData);
        socket.write(response);
        console.log('Sent response for cmLink');
      } else {
        console.error(`Unsupported command: 0x${cmd.toString(16)}`);
        // Implementation to send error response can be added as needed.
      }
    }
  });

  socket.on('close', () => {
    console.log('Client disconnected.');
  });

  socket.on('error', (err) => {
    console.error('Socket error:', err);
  });
});

server.listen(3001, () => {
  console.log('Device emulator is running on port 3001.');
});
