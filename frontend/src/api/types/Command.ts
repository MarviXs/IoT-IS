import type { paths } from '@/api/generated/schema.d.ts';

export type CommandsQueryParams = paths['/commands']['get']['parameters']['query'];
export type CommandsResponse = paths['/commands']['get']['responses']['200']['content']['application/json'];

export type CommandResponse = paths['/commands/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateCommandRequest = paths['/commands']['post']['requestBody']['content']['application/json'];
export type UpdateCommandRequest = paths['/commands/{id}']['put']['requestBody']['content']['application/json'];
