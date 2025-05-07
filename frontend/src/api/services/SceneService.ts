import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ScenesQueryParams = paths['/scenes']['get']['parameters']['query'];
export type ScenesPaginatedResponse = paths['/scenes']['get']['responses']['200']['content']['application/json'];
export type CreateSceneData = paths['/scenes']['post']['requestBody']['content']['application/json'];
export type UpdateSceneData = paths['/scenes/{id}']['put']['requestBody']['content']['application/json'];

class SceneService {
  async getScenes(query: ScenesQueryParams) {
    return await client.GET('/scenes', { params: { query: query } });
  }

  async getScene(id: string) {
    return await client.GET('/scenes/{id}', { params: { path: { id: id } } });
  }

  async createScene(data: CreateSceneData) {
    return await client.POST('/scenes', { body: data });
  }

  async updateScene(id: string, data: UpdateSceneData) {
    return await client.PUT('/scenes/{id}', { params: { path: { id: id } }, body: data });
  }

  async enableScene(id: string, enable: boolean) {
    return await client.PATCH('/scenes/{id}/enable', { params: { path: { id: id } }, body: { isEnabled: enable } });
  }

  async deleteScene(id: string) {
    return await client.DELETE('/scenes/{id}', { params: { path: { id: id } } });
  }
}

export default new SceneService();
