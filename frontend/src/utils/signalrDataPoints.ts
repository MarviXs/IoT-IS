import type { HubConnection } from '@microsoft/signalr';
import type { LastDataPoint } from '@/models/LastDataPoint';

const singleEventName = 'ReceiveSensorLastDataPoint';
const batchEventName = 'ReceiveSensorLastDataPoints';
const deviceSubscriptionCounts = new Map<string, number>();
const connectionsWithResubscribeHandler = new WeakSet<HubConnection>();

function ensureDeviceResubscribeHandler(connection: HubConnection) {
  if (connectionsWithResubscribeHandler.has(connection)) {
    return;
  }

  connection.onreconnected(() => {
    void resubscribeToDeviceDataPoints(connection);
  });
  connectionsWithResubscribeHandler.add(connection);
}

function subscribeToLastDataPointEvents(connection: HubConnection, onDataPoint: (dataPoint: LastDataPoint) => void) {
  const handleBatch = (dataPoints: LastDataPoint[]) => {
    for (const dataPoint of dataPoints) {
      onDataPoint(dataPoint);
    }
  };

  connection.on(singleEventName, onDataPoint);
  connection.on(batchEventName, handleBatch);

  return () => {
    connection.off(singleEventName, onDataPoint);
    connection.off(batchEventName, handleBatch);
  };
}

function upsertLastDataPoint(lastDataPoints: LastDataPoint[], dataPoint: LastDataPoint) {
  const index = lastDataPoints.findIndex((item) => item.deviceId === dataPoint.deviceId && item.tag === dataPoint.tag);

  if (index !== -1) {
    lastDataPoints[index] = dataPoint;
    return;
  }

  lastDataPoints.push(dataPoint);
}

async function subscribeToDeviceDataPoints(connection: HubConnection, deviceId: string) {
  ensureDeviceResubscribeHandler(connection);
  const currentCount = deviceSubscriptionCounts.get(deviceId) ?? 0;
  deviceSubscriptionCounts.set(deviceId, currentCount + 1);

  if (currentCount === 0 && connection.state === 'Connected') {
    await connection.send('SubscribeToDevice', deviceId);
  }
}

async function unsubscribeFromDeviceDataPoints(connection: HubConnection, deviceId: string) {
  const currentCount = deviceSubscriptionCounts.get(deviceId) ?? 0;

  if (currentCount <= 1) {
    deviceSubscriptionCounts.delete(deviceId);
    if (currentCount === 1 && connection.state === 'Connected') {
      await connection.send('UnsubscribeFromDevice', deviceId);
    }
    return;
  }

  deviceSubscriptionCounts.set(deviceId, currentCount - 1);
}

async function resubscribeToDeviceDataPoints(connection: HubConnection) {
  if (connection.state !== 'Connected') {
    return;
  }

  await Promise.all(
    Array.from(deviceSubscriptionCounts.keys()).map((deviceId) => connection.send('SubscribeToDevice', deviceId)),
  );
}

export {
  subscribeToLastDataPointEvents,
  subscribeToDeviceDataPoints,
  unsubscribeFromDeviceDataPoints,
  resubscribeToDeviceDataPoints,
  upsertLastDataPoint,
};
