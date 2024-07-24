import { client } from '@/api/client';
import { UpdateDeviceTemplateSensorsRequest } from '../types/Sensor';
class SensorService {
  async updateTemplateSensors(templateId: string, body: UpdateDeviceTemplateSensorsRequest[]) {
    return await client.PUT('/device-templates/{templateId}/sensors', { body, params: { path: { templateId } } });
  }

  async getTemplateSensors(templateId: string) {
    return await client.GET('/device-templates/{templateId}/sensors', { params: { path: { templateId } } });
  }
}

export default new SensorService();
