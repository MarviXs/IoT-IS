import { client } from '@/api/client';
import {
  CreateCollectionRequest,
  DeviceCollectionQueryParams,
  UpdateCollectionRequest,
} from '../types/DeviceCollection';

class DeviceCollectionService {
  async getCollections(queryParams: DeviceCollectionQueryParams) {
    return await client.GET('/device-collections', { params: { query: queryParams } });
  }

  async getCollection(id: string, maxDepth?: number) {
    return await client.GET('/device-collections/{collectionId}', {
      params: { path: { collectionId: id }, query: { MaxDepth: maxDepth } },
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
}

export default new DeviceCollectionService();
