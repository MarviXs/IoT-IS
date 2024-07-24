import type { paths, components } from '@/api/generated/schema.d.ts';

export type UpdateRecipeStepsRequest =
  paths['/recipes/{recipeId}/steps']['put']['requestBody']['content']['application/json'];

export type RecipeStep = components['schemas']['Fei.Is.Api.Features.Recipes.Queries.GetRecipeById.RecipeStepResponse'];
