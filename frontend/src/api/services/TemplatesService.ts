import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

class TemplatesService {
  async updateTemplate(formData: FormData) {
    return await client.PUT('/templates', {
      body: formData,
    });
  }
}

export default new TemplatesService();
