import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type SuppliersListQueryParams = paths['/suppliers']['get']['parameters']['query'];
export type SuppliersListResponse = paths['/suppliers']['get']['responses']['200']['content']['application/json'];

export type SupplierResponse = paths['/suppliers/{id}']['get']['responses']['200']['content']['application/json'];

class SupplierService {
  async getSuppliersList(queryParams: SuppliersListQueryParams) {
    return await client.GET('/suppliers', { params: { query: queryParams } });
  }

  async getSupplier(supplierId: string) {
    return await client.GET('/suppliers/{id}', { params: { path: { id: supplierId } } });
  }
}

export default new SupplierService();
