import { client } from '@/api/client';
import { CreateDeviceRequest, DevicesQueryParams, UpdateDeviceRequest } from '../types/Device';

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
