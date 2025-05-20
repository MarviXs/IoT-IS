import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type LifeCyclesQueryParams = paths['/lifecycles']['get']['parameters']['query'];
export type LifeCyclesResponse = paths['/lifecycles']['get']['responses']['200']['content']['application/json'];

export type PlantsResponse = paths['/lifecycles/{plantBoardId}']['get']['responses']['200']['content']['application/json'];
export type ProductResponse = paths['/lifecycles/plant/{plantId}']['get']['responses']['200']['content']['application/json'];
export type CreatePlantBoardRequest = paths['/lifecycles/plantboards']['post']['requestBody']['content']['application/json'];
export type CreatePlantRequest = paths['/lifecycles/plants']['post']['requestBody']['content']['application/json'];
export type CreateAnalysisRequest = paths['/lifecycles']['post']['requestBody']['content']['application/json'];

export type LifeBoardsQueryParams = paths['/lifeboards']['get']['parameters']['query'];
export type LifeBoardsResponse = paths['/lifeboards']['get']['responses']['200']['content']['application/json'];

export type PlantsCollectionQueryParams = paths['/lifecycles/{plantBoardId}']['get']['parameters']['query'];

class LifeCycleService {
  // Načítanie životných cyklov s podporou stránkovania a filtrovania
  async getLifeCycles(queryParams: LifeCyclesQueryParams) {
    return await client.GET('/lifecycles', { params: { query: queryParams } });
  }

  async getLifeCyclesByPlantId(plantId: string) {
    return await client.GET('/lifecycles/plant/{plantId}', { params: { path: { plantId } } });
  }

  /*async getPlantsByBoard(plantBoardId: string, queryParams: PlantsCollectionQueryParams) {
    return await client.GET('/lifecycles/{plantBoardId}', { params: { path: { plantBoardId }, queryParams } });
  }*/
    async getPlantsByBoard(plantBoardId: string, queryParams: PlantsCollectionQueryParams) {
      console.log("Sending queryParams:", queryParams);
      return await client.GET('/lifecycles/{plantBoardId}', { params: { path: { plantBoardId }, query: queryParams } });
    }

  async getPlantBoards(queryParams: LifeBoardsQueryParams) {
    return await client.GET('/lifeboards', { params: { query: queryParams } });
  }

  async deletePlant(id: string) {
    return await client.DELETE('/lifecycles/plant/{id}', { params: { path: { id } } });
  }

  async deletePlantboard(id: string) {
    return await client.DELETE('/lifeboard/{id}', { params: { path: { id } } });
  }

  async createPlant(request: CreatePlantRequest) {
    return await client.POST('/lifecycles/plants', { body: request });
  }

  async createAnalysis(request: CreateAnalysisRequest) {
    return await client.POST('/lifecycles', { body: request });
  }

  async createPlantBoard(request: CreatePlantBoardRequest) {
    return await client.POST('/lifecycles/plantboards', { body: request });
  }
}

export default new LifeCycleService();
