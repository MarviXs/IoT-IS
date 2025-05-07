import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

export type NotificationQueryParams = paths['/scene-notifications']['get']['parameters']['query'];
export type NotificationsPaginated =
  paths['/scene-notifications']['get']['responses']['200']['content']['application/json'];

class NotificationService {
  async getNotifications(query: NotificationQueryParams) {
    return await client.GET('/scene-notifications', { params: { query: query } });
  }
}

export default new NotificationService();
