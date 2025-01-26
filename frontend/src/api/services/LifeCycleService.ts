import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type LifeCyclesQueryParams = paths['/lifecycles']['get']['parameters']['query'];
export type LifeCyclesResponse = paths['/lifecycles']['get']['responses']['200']['content']['application/json'];

export type ProductResponse = paths['/lifecycles/plant/{plantId}']['get']['responses']['200']['content']['application/json'];
export type CreatePlantRequest = paths['/lifecycles/plants']['post']['requestBody']['content']['application/json'];


class LifeCycleService {
  // Načítanie životných cyklov s podporou stránkovania a filtrovania
  async getLifeCycles(queryParams: LifeCyclesQueryParams) {
    return await client.GET('/lifecycles', { params: { query: queryParams } });
  }

  async getLifeCyclesByPlantId(plantId: string) {
    return await client.GET('/lifecycles/plant/{plantId}', { params: { path: { plantId } } });
  }

  async deletePlant(id: string) {
    return await client.DELETE('/lifecycles/plant/{id}', { params: { path: { id } } });
  }

  async createPlant(request: CreatePlantRequest) {
    return await client.POST('/lifecycles/plants', { body: request });
  }
}

export default new LifeCycleService();
