import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type DeviceCollectionQueryParams = paths['/device-collections']['get']['parameters']['query'];
export type DeviceCollectionsResponse =
  paths['/device-collections']['get']['responses']['200']['content']['application/json'];
export type DeviceCollectionResponse =
  paths['/device-collections/{collectionId}']['get']['responses']['200']['content']['application/json'];
export type DeviceCollectionWithSensorsResponse =
  paths['/device-collections/{collectionId}/sensors']['get']['responses']['200']['content']['application/json'];
export type CreateCollectionRequest =
  paths['/device-collections']['post']['requestBody']['content']['application/json'];
export type CreateCollectionResponse =
  paths['/device-collections']['post']['responses']['201']['content']['application/json'];
export type UpdateCollectionRequest =
  paths['/device-collections/{id}']['put']['requestBody']['content']['application/json'];
export type UpdateCollectionResponse =
  paths['/device-collections/{id}']['put']['responses']['200']['content']['application/json'];

class DeviceCollectionService {
  async getCollections(queryParams: DeviceCollectionQueryParams) {
    return await client.GET('/device-collections', { params: { query: queryParams } });
  }

  async getCollection(id: string, maxDepth?: number) {
    return await client.GET('/device-collections/{collectionId}', {
      params: { path: { collectionId: id }, query: { MaxDepth: maxDepth } },
    });
  }

  async getCollectionWithSensors(id: string) {
    return await client.GET('/device-collections/{collectionId}/sensors', {
      params: { path: { collectionId: id } },
    });
  }

  async createCollection(data: CreateCollectionRequest) {
    return await client.POST('/device-collections', { body: data });
  }

  async updateCollection(id: string, data: UpdateCollectionRequest) {
    return await client.PUT('/device-collections/{id}', { params: { path: { id: id } }, body: data });
  }

  async deleteCollection(id: string) {
    return await client.DELETE('/device-collections/{id}', { params: { path: { id: id } } });
  }

  async addDeviceToCollection(collectionId: string, deviceId: string) {
    return await client.POST('/device-collections/{collectionId}/devices/{deviceId}', {
      params: { path: { collectionId, deviceId } },
    });
  }

  async removeDeviceFromCollection(collectionId: string, deviceId: string) {
    return await client.DELETE('/device-collections/{collectionId}/devices/{deviceId}', {
      params: { path: { collectionId, deviceId } },
    });
  }
}

export default new DeviceCollectionService();
