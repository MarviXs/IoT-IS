import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type DevicesQueryParams = paths['/devices']['get']['parameters']['query'];
export type DevicesResponse = paths['/devices']['get']['responses']['200']['content']['application/json'];
export type DeviceResponse = paths['/devices/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateDeviceRequest = paths['/devices']['post']['requestBody']['content']['application/json'];
export type UpdateDeviceRequest = paths['/devices/{id}']['put']['requestBody']['content']['application/json'];

class DeviceService {
  async getDevices(queryParams: DevicesQueryParams) {
    return await client.GET('/devices', { params: { query: queryParams } });
  }

  async getDevice(deviceTemplateId: string) {
    return await client.GET('/devices/{id}', { params: { path: { id: deviceTemplateId } } });
  }

  async createDevice(body: CreateDeviceRequest) {
    return await client.POST('/devices', { body });
  }

  async updateDevice(deviceId: string, body: UpdateDeviceRequest) {
    return await client.PUT('/devices/{id}', { body, params: { path: { id: deviceId } } });
  }

  async deleteDevice(deviceId: string) {
    return await client.DELETE('/devices/{id}', { params: { path: { id: deviceId } } });
  }
}

export default new DeviceService();
