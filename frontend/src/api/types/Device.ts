import type { paths } from '@/api/generated/schema.d.ts';

export type DevicesQueryParams = paths['/devices']['get']['parameters']['query'];
export type DevicesResponse = paths['/devices']['get']['responses']['200']['content']['application/json'];

export type DeviceResponse = paths['/devices/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateDeviceRequest = paths['/devices']['post']['requestBody']['content']['application/json'];

export type UpdateDeviceRequest = paths['/devices/{id}']['put']['requestBody']['content']['application/json'];
