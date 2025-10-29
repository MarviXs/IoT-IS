import { client } from '@/api/client';
import type { components } from '@/api/generated/schema.d.ts';

export type DeviceControlResponse =
  components['schemas']['Fei.Is.Api.Features.DeviceControls.Queries.GetDeviceControls.Response'];
export type UpdateDeviceTemplateControlsRequest =
  components['schemas']['Fei.Is.Api.Features.DeviceControls.Commands.UpdateDeviceTemplateControls.Request'];

class DeviceControlService {
  async getDeviceControls(deviceId: string) {
    return await client.GET('/devices/{deviceId}/controls', {
      params: { path: { deviceId } },
    });
  }

  async getTemplateControls(templateId: string) {
    return await this.getDeviceControls(templateId);
  }

  async updateTemplateControls(templateId: string, body: UpdateDeviceTemplateControlsRequest[]) {
    return await client.PUT('/device-templates/{templateId}/controls', {
      body,
      params: { path: { templateId } },
    });
  }
}

export default new DeviceControlService();
