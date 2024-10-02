import { client } from '@/api/client';
import type { components } from '@/api/generated/schema.d.ts';

export type UpdateDeviceTemplateSensorsRequest =
  components['schemas']['Fei.Is.Api.Features.Sensors.Commands.UpdateDeviceTemplateSensors.Request'];
export type Sensor = components['schemas']['Fei.Is.Api.Features.Sensors.Queries.GetSensorById.Response'];

class SensorService {
  async updateTemplateSensors(templateId: string, body: UpdateDeviceTemplateSensorsRequest[]) {
    return await client.PUT('/device-templates/{templateId}/sensors', { body, params: { path: { templateId } } });
  }

  async getTemplateSensors(templateId: string) {
    return await client.GET('/device-templates/{templateId}/sensors', { params: { path: { templateId } } });
  }
}

export default new SensorService();
