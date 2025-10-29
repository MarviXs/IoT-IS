import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type AdminDevicesQueryParams = paths['/admin/devices']['get']['parameters']['query'];
export type AdminDevicesResponse = paths['/admin/devices']['get']['responses']['200']['content']['application/json'];
export type ChangeDeviceOwnerRequest = paths['/admin/devices/{id}/owner']['put']['requestBody']['content']['application/json'];

class AdminDeviceService {
  async getDevices(queryParams: AdminDevicesQueryParams) {
    return await client.GET('/admin/devices', { params: { query: queryParams } });
  }

  async updateOwner(deviceId: string, body: ChangeDeviceOwnerRequest) {
    return await client.PUT('/admin/devices/{id}/owner', {
      params: { path: { id: deviceId } },
      body,
    });
  }
}

export default new AdminDeviceService();
