import type { HubConnection } from '@microsoft/signalr';
import type { LastDataPoint } from '@/models/LastDataPoint';

const singleEventName = 'ReceiveSensorLastDataPoint';
const batchEventName = 'ReceiveSensorLastDataPoints';

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

export { subscribeToLastDataPointEvents, upsertLastDataPoint };
