import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type GetDataPointsQuery = paths['/devices/{deviceId}/sensors/{sensorTag}/data']['get']['parameters']['query'];
export type GetDataPointsResponse =
  paths['/devices/{deviceId}/sensors/{sensorTag}/data']['get']['responses']['200']['content']['application/json'];
export type GetLatestDataPointsResponse =
  paths['/devices/{deviceId}/sensors/{sensorTag}/data/latest']['get']['responses']['200']['content']['application/json'];

class DataPointService {
  async getDataPoints(deviceId: string, sensorTag: string, query: GetDataPointsQuery) {
    return await client.GET('/devices/{deviceId}/sensors/{sensorTag}/data', {
      params: { path: { deviceId, sensorTag }, query },
    });
  }

  async getLatestDataPoints(deviceId: string, sensorTag: string) {
    return await client.GET('/devices/{deviceId}/sensors/{sensorTag}/data/latest', {
      params: { path: { deviceId, sensorTag } },
    });
  }
}

export default new DataPointService();
