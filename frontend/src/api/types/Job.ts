import type { paths } from '@/api/generated/schema.d.ts';

export type ActiveJobResponse =
  paths['/devices/{deviceId}/jobs/active']['get']['responses']['200']['content']['application/json'];

export type StartJobRequest = paths['/devices/{deviceId}/jobs']['post']['requestBody']['content']['application/json'];
