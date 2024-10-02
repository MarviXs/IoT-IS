import RecipeService from '@/api/services/RecipeService';
import { RecipeStep } from '@/api/services/RecipeService';
import { SubRecipeNode } from '@/models/SubrecipeNode';

function subrecipeToNodes(recipeStep: RecipeStep, root = false): SubRecipeNode {
  if (recipeStep.subrecipe) {
    return {
      id: recipeStep.subrecipe.id,
      label: recipeStep.subrecipe?.name,
      cycles: recipeStep.cycles ?? 1,
      children: recipeStep.subrecipe?.steps?.map((subStep) => subrecipeToNodes(subStep)) ?? [],
      isSubrecipe: true,
      lazy: true,
      root: root,
    };
  } else {
    return {
      id: recipeStep.command?.id,
      label: recipeStep.command?.displayName,
      children: [],
      cycles: recipeStep.cycles ?? 1,
      lazy: false,
      root: root,
    };
  }
}

async function lazyLoadSubrecipe({
  node,
  key,
  done,
  fail,
}: {
  node: SubRecipeNode;
  key: string | number;
  done: (children: SubRecipeNode[]) => void;
  fail: (error: unknown) => void;
}) {
  if (!node.id) return;

  const { data, error } = await RecipeService.getRecipe(node.id);
  if (error) {
    fail(error);
    return;
  }

  const nodes = data.steps?.map((subStep) => subrecipeToNodes(subStep)) ?? [];
  done(nodes);
}

export { subrecipeToNodes, lazyLoadSubrecipe };
