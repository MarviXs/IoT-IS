import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type AdminDevicesQueryParams = paths['/admin/devices']['get']['parameters']['query'];
export type AdminDevicesResponse = paths['/admin/devices']['get']['responses']['200']['content']['application/json'];

class AdminDeviceService {
  async getDevices(queryParams: AdminDevicesQueryParams) {
    return await client.GET('/admin/devices', { params: { query: queryParams } });
  }
}

export default new AdminDeviceService();
