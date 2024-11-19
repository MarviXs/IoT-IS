import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type VATCategoryQueryParams = paths['/vat-category']['get']['parameters']['query'];
export type VATCategoryResponse = paths['/vat-category']['get']['responses']['200']['content']['application/json'];

class VATCategoryService {
  async getVATCategories(queryParams: VATCategoryQueryParams) {
    return await client.GET('/vat-category', { params: { query: queryParams } });
  }
}

export default new VATCategoryService();
