import type { paths } from '@/api/generated/schema.d.ts';

export type DeviceTemplatesQueryParams = paths['/device-templates']['get']['parameters']['query'];
export type DeviceTemplatesResponse =
  paths['/device-templates']['get']['responses']['200']['content']['application/json'];

export type DeviceTemplateResponse =
  paths['/device-templates/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateDeviceTemplateRequest =
  paths['/device-templates']['post']['requestBody']['content']['application/json'];

export type UpdateDeviceTemplateRequest =
  paths['/device-templates/{id}']['put']['requestBody']['content']['application/json'];
