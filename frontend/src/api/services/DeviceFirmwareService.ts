import { client } from '@/api/client';
import type { components } from '@/api/generated/schema.d.ts';

type DeviceFirmwareListResponse =
  components['schemas']['Fei.Is.Api.Features.DeviceFirmwares.Queries.GetDeviceFirmwares.Response'];
type DeviceFirmwareDetailResponse =
  components['schemas']['Fei.Is.Api.Features.DeviceFirmwares.Queries.GetDeviceFirmwareById.Response'];

type CreateDeviceFirmwareRequest =
  components['schemas']['Fei.Is.Api.Features.DeviceFirmwares.Commands.CreateDeviceFirmware.Request'];
type UpdateDeviceFirmwareRequest =
  components['schemas']['Fei.Is.Api.Features.DeviceFirmwares.Commands.UpdateDeviceFirmware.Request'];
export type DeviceFirmwareResponse = DeviceFirmwareListResponse;
export type DeviceFirmwareDetail = DeviceFirmwareDetailResponse;

export type DeviceFirmwareFormPayload = {
  versionNumber: string;
  isActive: boolean;
  firmwareFile: File | null;
};

function mapToFormData(payload: Pick<CreateDeviceFirmwareRequest, 'versionNumber' | 'isActive'> & {
  firmwareFile?: File | null;
}) {
  const formData = new FormData();
  formData.append('versionNumber', payload.versionNumber);
  formData.append('isActive', payload.isActive ? 'true' : 'false');

  if (payload.firmwareFile) {
    formData.append('firmwareFile', payload.firmwareFile);
  }

  return formData;
}

class DeviceFirmwareService {
  async getDeviceFirmwares(templateId: string) {
    return await client.GET('/device-templates/{templateId}/firmwares', {
      params: { path: { templateId } },
    });
  }

  async getDeviceFirmware(templateId: string, firmwareId: string) {
    return await client.GET('/device-templates/{templateId}/firmwares/{firmwareId}', {
      params: { path: { templateId, firmwareId } },
    });
  }

  async createDeviceFirmware(templateId: string, payload: DeviceFirmwareFormPayload) {
    const { versionNumber, isActive, firmwareFile } = payload;
    if (!firmwareFile) {
      throw new Error('Firmware file is required');
    }

    const body = mapToFormData({ versionNumber, isActive, firmwareFile });

    return await client.POST('/device-templates/{templateId}/firmwares', {
      params: { path: { templateId } },
      body: body as unknown as CreateDeviceFirmwareRequest,
    });
  }

  async updateDeviceFirmware(templateId: string, firmwareId: string, payload: DeviceFirmwareFormPayload) {
    const { versionNumber, isActive, firmwareFile } = payload;
    const body = mapToFormData({ versionNumber, isActive, firmwareFile: firmwareFile ?? undefined });

    return await client.PUT('/device-templates/{templateId}/firmwares/{firmwareId}', {
      params: { path: { templateId, firmwareId } },
      body: body as unknown as UpdateDeviceFirmwareRequest,
    });
  }

  async deleteDeviceFirmware(templateId: string, firmwareId: string) {
    return await client.DELETE('/device-templates/{templateId}/firmwares/{firmwareId}', {
      params: { path: { templateId, firmwareId } },
    });
  }
}

export default new DeviceFirmwareService();
