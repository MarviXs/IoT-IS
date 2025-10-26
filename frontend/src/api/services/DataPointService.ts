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
export type GetDataPointCountQuery =
  operations['GetSensorDataPointsCount']['parameters']['query'];
export type GetDataPointCountResponse =
  operations['GetSensorDataPointsCount']['responses']['200']['content']['application/json'];
export type DeleteDataPointsQuery =
  operations['DeleteSensorDataPoints']['parameters']['query'];
export type DeleteDataPointsResponse =
  operations['DeleteSensorDataPoints']['responses']['200']['content']['application/json'];

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

  async getDataPointCount(deviceId: string, sensorTag: string, query?: GetDataPointCountQuery) {
    return await client.GET('/devices/{deviceId}/sensors/{sensorTag}/data/count', {
      params: { path: { deviceId, sensorTag }, query },
    });
  }

  async deleteDataPoints(deviceId: string, sensorTag: string, query?: DeleteDataPointsQuery) {
    return await client.DELETE('/devices/{deviceId}/sensors/{sensorTag}/data', {
      params: { path: { deviceId, sensorTag }, query },
    });
  }
}

export default new DataPointService();
