import { client } from '@/api/client';
import type { components } from '@/api/generated/schema.d.ts';

class SceneService {
  async getScenes(query: any) {
    return await client.GET('/device-templates', { params: query }); // TODO: change to scenes
  }
}

export default new SceneService();
