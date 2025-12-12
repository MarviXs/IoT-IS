import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type DeviceTemplatesQueryParams = paths['/device-templates']['get']['parameters']['query'];
export type DeviceTemplatesResponse =
  paths['/device-templates']['get']['responses']['200']['content']['application/json'];
export type DeviceTemplateResponse =
  paths['/device-templates/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateDeviceTemplateRequest =
  paths['/device-templates']['post']['requestBody']['content']['application/json'];
export type UpdateDeviceTemplateRequest =
  paths['/device-templates/{id}']['put']['requestBody']['content']['application/json'];

export type ImportDeviceTemplateRequest =
  paths['/device-templates/import']['post']['requestBody']['content']['application/json'];
export type ExportDeviceTemplateResponse =
  paths['/device-templates/{id}/export']['get']['responses']['200']['content']['application/json'];

class DeviceTemplateService {
  async getDeviceTemplates(queryParams: DeviceTemplatesQueryParams) {
    return await client.GET('/device-templates', { params: { query: queryParams } });
  }

  async getDeviceTemplate(deviceTemplateId: string) {
    return await client.GET('/device-templates/{id}', { params: { path: { id: deviceTemplateId } } });
  }

  async createDeviceTemplate(body: CreateDeviceTemplateRequest) {
    return await client.POST('/device-templates', { body });
  }

  async updateDeviceTemplate(deviceTemplateId: string, body: UpdateDeviceTemplateRequest) {
    return await client.PUT('/device-templates/{id}', { body, params: { path: { id: deviceTemplateId } } });
  }

  async deleteDeviceTemplate(deviceTemplateId: string) {
    return await client.DELETE('/device-templates/{id}', { params: { path: { id: deviceTemplateId } } });
  }

  async importDeviceTemplate(body: ImportDeviceTemplateRequest) {
    return await client.POST('/device-templates/import', { body });
  }

  async exportDeviceTemplate(deviceTemplateId: string) {
    return await client.GET('/device-templates/{id}/export', { params: { path: { id: deviceTemplateId } } });
  }
}

export default new DeviceTemplateService();
