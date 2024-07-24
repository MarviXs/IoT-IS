interface SubRecipeNode {
  root?: boolean;
  id?: string;
  label?: string;
  cycles: number;
  isSubrecipe?: boolean;
  children?: SubRecipeNode[];
  lazy?: boolean;
}

export type { SubRecipeNode };
