import { client } from '@/api/client';
import {
  CreateDeviceTemplateRequest,
  DeviceTemplatesQueryParams,
  UpdateDeviceTemplateRequest,
} from '../types/DeviceTemplate';

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
}

export default new DeviceTemplateService();
