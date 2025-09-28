import { client } from '@/api/client';
import type { operations, paths } from '@/api/generated/schema.d.ts';

export type GetDataPointsQuery =
  operations['GetSensorDataPoints']['parameters']['query'];
export type GetDataPointsResponse =
  operations['GetSensorDataPoints']['responses']['200']['content']['application/json'];
export type GetLatestDataPointsQuery =
  operations['GetLatestDataPoints']['parameters']['query'];
export type GetLatestDataPointsResponse =
  paths['/devices/{deviceId}/sensors/{sensorTag}/data/latest']['get']['responses']['200']['content']['application/json'];

class DataPointService {
  async getDataPoints(deviceId: string, sensorTag: string, query: GetDataPointsQuery) {
    return await client.GET('/devices/{deviceId}/sensors/{sensorTag}/data', {
      params: { path: { deviceId, sensorTag }, query },
    });
  }

  async getLatestDataPoints(deviceId: string, sensorTag: string, query?: GetLatestDataPointsQuery) {
    return await client.GET('/devices/{deviceId}/sensors/{sensorTag}/data/latest', {
      params: { path: { deviceId, sensorTag }, query },
    });
  }
}

export default new DataPointService();
