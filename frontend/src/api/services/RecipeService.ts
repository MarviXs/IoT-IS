import { client } from '@/api/client';
import type { paths, components } from '@/api/generated/schema.d.ts';

export type RecipesQueryParams = paths['/recipes']['get']['parameters']['query'];
export type RecipesResponse = paths['/recipes']['get']['responses']['200']['content']['application/json'];
export type RecipeResponse = paths['/recipes/{id}']['get']['responses']['200']['content']['application/json'];
export type CreateRecipeRequest = paths['/recipes']['post']['requestBody']['content']['application/json'];
export type UpdateRecipeRequest = paths['/recipes/{id}']['put']['requestBody']['content']['application/json'];
export type UpdateRecipeStepsRequest =
  paths['/recipes/{recipeId}/steps']['put']['requestBody']['content']['application/json'];
export type RecipeStep = components['schemas']['Fei.Is.Api.Features.Recipes.Queries.GetRecipeById.RecipeStepResponse'];

class RecipeService {
  async getRecipes(queryParams: RecipesQueryParams) {
    return await client.GET('/recipes', { params: { query: queryParams } });
  }

  async getRecipe(id: string) {
    return await client.GET('/recipes/{id}', { params: { path: { id: id } } });
  }

  async createRecipe(createCommandRequest: CreateRecipeRequest) {
    return await client.POST('/recipes', { body: createCommandRequest });
  }

  async updateRecipe(id: string, updateCommandRequest: UpdateRecipeRequest) {
    return await client.PUT('/recipes/{id}', { body: updateCommandRequest, params: { path: { id: id } } });
  }

  async deleteRecipe(id: string) {
    return await client.DELETE('/recipes/{id}', { params: { path: { id: id } } });
  }

  async updateRecipeSteps(recipeId: string, body: UpdateRecipeStepsRequest) {
    return await client.PUT('/recipes/{recipeId}/steps', { body, params: { path: { recipeId } } });
  }
}

export default new RecipeService();
