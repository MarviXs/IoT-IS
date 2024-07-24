import type { paths } from '@/api/generated/schema.d.ts';

export type RecipesQueryParams = paths['/recipes']['get']['parameters']['query'];
export type RecipesResponse = paths['/recipes']['get']['responses']['200']['content']['application/json'];

export type RecipeResponse = paths['/recipes/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateRecipeRequest = paths['/recipes']['post']['requestBody']['content']['application/json'];
export type UpdateRecipeRequest = paths['/recipes/{id}']['put']['requestBody']['content']['application/json'];
