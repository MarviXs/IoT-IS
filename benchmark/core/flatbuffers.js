function encodedLength(value) {
  return value.length;
}

function encodeString(value) {
  const bytes = new Uint8Array(value.length);

  for (let index = 0; index < value.length; index += 1) {
    const codePoint = value.charCodeAt(index);
    if (codePoint > 0x7f) {
      throw new Error("FlatBuffer encoder only supports ASCII strings.");
    }

    bytes[index] = codePoint;
  }

  return bytes;
}

export function encodeAsciiString(value) {
  return encodeString(value);
}

function writeInt32(buffer, offset, value) {
  buffer[offset] = value & 0xff;
  buffer[offset + 1] = (value >>> 8) & 0xff;
  buffer[offset + 2] = (value >>> 16) & 0xff;
  buffer[offset + 3] = (value >>> 24) & 0xff;
}

function writeInt16(buffer, offset, value) {
  buffer[offset] = value & 0xff;
  buffer[offset + 1] = (value >>> 8) & 0xff;
}

function writeInt64(buffer, offset, value) {
  const low = value >>> 0;
  const high = Math.floor(value / 0x100000000) >>> 0;

  writeInt32(buffer, offset, low);
  writeInt32(buffer, offset + 4, high);
}

function writeFloat64(buffer, offset, value) {
  const arrayBuffer = new ArrayBuffer(8);
  const dataView = new DataView(arrayBuffer);
  dataView.setFloat64(0, value, true);

  buffer.set(new Uint8Array(arrayBuffer), offset);
}

function alignTo(value, alignment) {
  const remainder = value % alignment;
  return remainder === 0 ? value : value + alignment - remainder;
}

export function createDataPointFlatBuffer(tag, value, timestamp) {
  const tagBytes = encodeString(tag);
  return createDataPointFlatBufferFromBytes(tagBytes, value, timestamp);
}

export function createDataPointFlatBufferFromBytes(tagBytes, value, timestamp) {
  const stringLength = 4 + tagBytes.length + 1;
  const stringStart = 48;
  const totalLength = alignTo(stringStart + stringLength, 4);
  const buffer = new Uint8Array(totalLength);

  // Root table offset
  writeInt32(buffer, 0, 24);

  // Vtable
  writeInt16(buffer, 4, 18);
  writeInt16(buffer, 6, 24);
  writeInt16(buffer, 8, 4);
  writeInt16(buffer, 10, 8);
  writeInt16(buffer, 12, 16);
  writeInt16(buffer, 14, 0);
  writeInt16(buffer, 16, 0);
  writeInt16(buffer, 18, 0);
  writeInt16(buffer, 20, 0);

  // Table
  writeInt32(buffer, 24, 20);
  writeInt32(buffer, 28, stringStart - 28);
  writeFloat64(buffer, 32, value);
  writeInt64(buffer, 40, timestamp);

  // String
  writeInt32(buffer, stringStart, tagBytes.length);
  buffer.set(tagBytes, stringStart + 4);
  buffer[stringStart + 4 + tagBytes.length] = 0;

  return buffer;
}

export function estimateDataPointPayloadSize(tag) {
  return alignTo(48 + 4 + encodedLength(tag) + 1, 4);
}
