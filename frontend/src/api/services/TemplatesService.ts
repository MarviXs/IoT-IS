import { client } from '@/api/client';
import { EDocumentIdentifier } from '../types/EDocumentIdentifier';

//export type SuppliersListQueryParams = paths['/templates']['get']['parameters']['query'];

class TemplatesService {
  async updateTemplate(formData: FormData) {
    return await client.PUT('/templates', {
      // @ts-ignore
      body: formData,
    });
  }
  async getTemplates() {
    return await client.GET('/templates');
  }
  async downloadTemplate(identifier: EDocumentIdentifier) {
    return await client.GET(`/templates/download`, {
      params: { query: { identifier: EDocumentIdentifier[identifier] } },
      parseAs: 'stream',
    });
  }
}

export default new TemplatesService();
