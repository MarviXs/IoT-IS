import { baseUrl, client, customFetch } from '@/api/client';
import type { components } from '@/api/generated/schema.d.ts';
import type { ProblemDetails } from '@/api/types/ProblemDetails';

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

function normalizeFirmwareFile(file: unknown): File | null {
  if (!file) {
    return null;
  }

  if (file instanceof File) {
    return file;
  }

  if (Array.isArray(file)) {
    return file.find((candidate): candidate is File => candidate instanceof File) ?? null;
  }

  if (typeof FileList !== 'undefined' && file instanceof FileList) {
    return file.length > 0 ? file.item(0) : null;
  }

  return null;
}

function mapToFormData(payload: Pick<CreateDeviceFirmwareRequest, 'versionNumber' | 'isActive'> & {
  firmwareFile?: unknown;
}) {
  const formData = new FormData();
  formData.append('versionNumber', payload.versionNumber);
  formData.append('isActive', payload.isActive ? 'true' : 'false');

  const firmwareFile = normalizeFirmwareFile(payload.firmwareFile);

  if (firmwareFile) {
    formData.append('firmwareFile', firmwareFile, firmwareFile.name);
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
    const normalizedFile = normalizeFirmwareFile(firmwareFile);

    if (!normalizedFile) {
      throw new Error('Firmware file is required');
    }

    const body = mapToFormData({ versionNumber, isActive, firmwareFile: normalizedFile });

    return await client.POST('/device-templates/{templateId}/firmwares', {
      params: { path: { templateId } },
      body: body as unknown as CreateDeviceFirmwareRequest,
    });
  }

  async updateDeviceFirmware(templateId: string, firmwareId: string, payload: DeviceFirmwareFormPayload) {
    const { versionNumber, isActive, firmwareFile } = payload;
    const body = mapToFormData({
      versionNumber,
      isActive,
      firmwareFile: normalizeFirmwareFile(firmwareFile ?? undefined) ?? undefined,
    });

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

  async downloadDeviceFirmware(downloadUrl: string) {
    try {
      const url = new URL(downloadUrl, baseUrl);
      const response = await customFetch(url.toString(), { method: 'GET' });

      if (!response.ok) {
        let error: ProblemDetails | null = null;

        try {
          if (response.headers.get('content-type')?.includes('application/json')) {
            error = (await response.clone().json()) as ProblemDetails;
          } else {
            const message = await response.clone().text();
            error = { title: message || 'Firmware download failed.' } as ProblemDetails;
          }
        } catch {
          error = { title: 'Firmware download failed.' } as ProblemDetails;
        }

        return { data: null, error } as const;
      }

      const blob = await response.blob();
      return { data: blob, error: null } as const;
    } catch {
      return {
        data: null,
        error: { title: 'Firmware download failed.' } as ProblemDetails,
      } as const;
    }
  }
}

export default new DeviceFirmwareService();
