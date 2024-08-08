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
