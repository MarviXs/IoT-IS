import type { paths } from '@/api/generated/schema.d.ts';

export type DeviceCollectionQueryParams = paths['/device-collections']['get']['parameters']['query'];

export type DeviceCollectionsResponse =
  paths['/device-collections']['get']['responses']['200']['content']['application/json'];

export type CreateCollectionRequest =
  paths['/device-collections']['post']['requestBody']['content']['application/json'];
