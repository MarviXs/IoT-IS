import { client } from '@/api/client';
import { RecipesQueryParams, CreateRecipeRequest, UpdateRecipeRequest } from '../types/Recipe';
import { UpdateRecipeStepsRequest } from '../types/RecipeStep';

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
