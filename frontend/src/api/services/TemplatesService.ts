import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

class TemplatesService {
  
  async updateTemplate(formData: FormData) {
    return await client.PUT('/templates', {
      // @ts-ignore
      body: formData,
    });
  }
  /*async getTemplates() {
    return await client.GET('/templates', {});
  }*/
  
 
}

export default new TemplatesService();
