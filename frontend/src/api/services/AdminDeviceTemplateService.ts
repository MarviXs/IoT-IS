import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema';

export type AdminDeviceTemplatesQueryParams = paths['/admin/device-templates']['get']['parameters']['query'];
export type AdminDeviceTemplatesResponse = paths['/admin/device-templates']['get']['responses']['200']['content']['application/json'];
export type ChangeDeviceTemplateOwnerRequest = paths['/admin/device-templates/{id}/owner']['put']['requestBody']['content']['application/json'];

class AdminDeviceTemplateService {
  async getDeviceTemplates(queryParams?: AdminDeviceTemplatesQueryParams) {
    return await client.GET('/admin/device-templates', {
      params: { query: queryParams },
    });
  }

  async updateOwner(templateId: string, body: ChangeDeviceTemplateOwnerRequest) {
    return await client.PUT('/admin/device-templates/{id}/owner', {
      params: { path: { id: templateId } },
      body,
    });
  }
}

export default new AdminDeviceTemplateService();
