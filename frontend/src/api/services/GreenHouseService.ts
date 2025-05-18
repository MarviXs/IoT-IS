import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type GreenHousesQueryParams = paths['/greenhouses']['get']['parameters']['query'];
export type GreenHousesResponse = paths['/greenhouses']['get']['responses']['200']['content']['application/json'];

export type CreateGreenHouseRequest = paths['/greenhouses']['post']['requestBody']['content']['application/json'];
export type GreenHouseDetailResponse = paths['/greenhouses/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateEditorPlantRequest = paths['/editorplants']['post']['requestBody']['content']['application/json'];
export type CreateEditorPlantResponse = paths['/editorplants']['post']['responses']['201']['content']['application/json'];

export type CreateEditorPotRequest = paths['/editorboards']['post']['requestBody']['content']['application/json'];
export type CreateEditorPotResponse = paths['/editorboards']['post']['responses']['201']['content']['application/json'];


class GreenHouseService {
  // Získanie zoznamu skleníkov s možnosťou filtrovania/paginácie
  async getGreenHouses(queryParams: GreenHousesQueryParams) {
    return await client.GET('/greenhouses', {
      params: { query: queryParams },
    });
  }

  async createGreenHouse(request: CreateGreenHouseRequest) {
    return await client.POST('/greenhouses', {
      body: request,
    });
  }

  async deleteGreenHouse(id: string) {
    return await client.DELETE('/greenhouses/{id}', {
      params: { path: { id } },
    });
  }

  async getGreenHouseById(id: string) {
    return await client.GET('/greenhouses/{id}', {
      params: { path: { id } },
    });
  }

  async getPlantsByGreenHouseId(greenhouseId: string) {
    return await client.GET('/greenhouses/{greenhouseId}/plants', {
      params: { path: { greenhouseId } },
    });
  }

  async getBoardsByGreenHouseId(greenhouseId: string) {
    return await client.GET('/greenhouses/{greenhouseId}/boards', {
      params: { path: { greenhouseId } },
    });
  }

  async createEditorPlants(request: CreateEditorPlantRequest) {
    return await client.POST('/editorplants', {
      body: request,
    });
  }

  async createEditorPot(request: CreateEditorPotRequest) {
    return await client.POST('/editorboards', {
      body: request,
    });
  }
}

export default new GreenHouseService();
