namespace Fei.Is.Api.Data.Models;

public class RecipeStep : BaseModel
{
    public required Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public Guid? CommandId { get; set; }
    public Command? Command { get; set; }
    public Guid? SubrecipeId { get; set; }
    public Recipe? Subrecipe { get; set; }
    public int Cycles { get; set; } = 1;
    public int Order { get; set; }

    public bool IsCommand => CommandId.HasValue;
    public bool IsSubRecipe => SubrecipeId.HasValue;
}
