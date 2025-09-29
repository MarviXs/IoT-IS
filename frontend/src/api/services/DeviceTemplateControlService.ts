import { client } from '@/api/client';
import type { components } from '@/api/generated/schema.d.ts';

export type DeviceTemplateControlResponse =
  components['schemas']['Fei.Is.Api.Features.DeviceControls.Queries.GetDeviceTemplateControls.Response'];
export type UpdateDeviceTemplateControlsRequest =
  components['schemas']['Fei.Is.Api.Features.DeviceControls.Commands.UpdateDeviceTemplateControls.Request'];

class DeviceTemplateControlService {
  async getTemplateControls(templateId: string) {
    return await client.GET('/device-templates/{templateId}/controls', {
      params: { path: { templateId } },
    });
  }

  async updateTemplateControls(templateId: string, body: UpdateDeviceTemplateControlsRequest[]) {
    return await client.PUT('/device-templates/{templateId}/controls', {
      body,
      params: { path: { templateId } },
    });
  }
}

export default new DeviceTemplateControlService();
