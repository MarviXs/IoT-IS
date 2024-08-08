import type { paths } from '@/api/generated/schema.d.ts';

export type UsersQueryParams = paths['/admin/users']['get']['parameters']['query'];
export type UsersResponse = paths['/admin/users']['get']['responses']['200']['content']['application/json'];

export type GetUserByIdResponse = paths['/admin/users/{id}']['get']['responses']['200']['content']['application/json'];
