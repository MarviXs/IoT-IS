import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type VATCategoryResponse = paths['/vat-category']['get']['responses']['200']['content']['application/json'];

class VATCategoryService {
  async getVATCategories() {
    return await client.GET('/vat-category');
  }
}

export default new VATCategoryService();
