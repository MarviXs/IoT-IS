import type { paths } from '@/api/generated/schema.d.ts';

export type LoginRequest = paths['/auth/login']['post']['requestBody']['content']['application/json'];
export type RegisterRequest = paths['/auth/register']['post']['requestBody']['content']['application/json'];
