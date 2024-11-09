import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ShareDeviceData = paths['/devices/{deviceId}/share']['post']['requestBody']['content']['application/json'];
export type UnshareDeviceData =
  paths['/devices/{deviceId}/unshare']['post']['requestBody']['content']['application/json'];
export type SharedUsers = paths['/devices/{id}/shared-users']['get']['responses']['200']['content']['application/json'];

class DeviceService {
  async shareDevice(body: ShareDeviceData, deviceId: string) {
    return await client.POST('/devices/{deviceId}/share', { body, params: { path: { deviceId } } });
  }

  async unshareDevice(body: UnshareDeviceData, deviceId: string) {
    return await client.POST('/devices/{deviceId}/unshare', { body, params: { path: { deviceId } } });
  }

  async getSharedUsers(deviceId: string) {
    return await client.GET('/devices/{id}/shared-users', { params: { path: { id: deviceId } } });
  }
}

export default new DeviceService();
