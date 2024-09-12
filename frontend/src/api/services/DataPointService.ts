import { client } from '@/api/client';
import { GetDataPointsQuery } from '../types/DataPoint';

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
