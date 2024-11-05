import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type ScenesQueryParames = paths['/scenes']['get']['parameters']['query'];
export type CreateSceneData = paths['/scenes']['post']['requestBody']['content']['application/json'];

class SceneService {
  async getScenes(query: any) {
    return await client.GET('/scenes', { params: query });
  }

  async createScene(data: CreateSceneData) {
    return await client.POST('/scenes', { body: data });
  }

  async deleteScene(id: string) {
    return await client.DELETE('/scenes/{id}', { params: { path: { id: id } } });
  }
}

export default new SceneService();
