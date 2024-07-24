import type { paths } from '@/api/generated/schema.d.ts';

export type GetDataPointsQuery = paths['/devices/{deviceId}/sensors/{sensorTag}/data']['get']['parameters']['query'];
export type GetDataPointsResponse =
  paths['/devices/{deviceId}/sensors/{sensorTag}/data']['get']['responses']['200']['content']['application/json'];
