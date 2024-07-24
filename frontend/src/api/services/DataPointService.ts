import { client } from '@/api/client';
import { GetDataPointsQuery } from '../types/DataPoint';

class DataPointService {
  async getDataPoints(deviceId: string, sensorTag: string, query: GetDataPointsQuery) {
    return await client.GET('/devices/{deviceId}/sensors/{sensorTag}/data', {
      params: { path: { deviceId, sensorTag }, query },
    });
  }
}

export default new DataPointService();
