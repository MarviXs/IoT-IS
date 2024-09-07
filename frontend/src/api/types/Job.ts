import type { components, paths } from '@/api/generated/schema.d.ts';

export type ActiveJobsResponse =
  paths['/devices/{deviceId}/jobs/active']['get']['responses']['200']['content']['application/json'];

export type StartJobRequest = paths['/devices/{deviceId}/jobs']['post']['requestBody']['content']['application/json'];

export type JobsQueryParams = paths['/jobs']['get']['parameters']['query'];

export type JobResponse = paths['/jobs/{jobId}']['get']['responses']['200']['content']['application/json'];
export type JobsResponse = paths['/jobs']['get']['responses']['200']['content']['application/json'];
