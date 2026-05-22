using Godot;
using System.Collections.Generic;
using dd2d.core;

namespace dd2d.restaurant.kitchen
{
    [GlobalClass]
    public partial class MenuManager : Resource
    {
        [Export]
        public Godot.Collections.Array<RecipeResource> Recipes { get; set; } = new();

        private static readonly RecipeResource[] DefaultRecipes = new[]
        {
            new RecipeResource { ItemName = "Burger",  BaseQuality = 2f, Difficulty = 1f, BasePreparationTime = 15f },
            new RecipeResource { ItemName = "Salad",   BaseQuality = 3f, Difficulty = 2f, BasePreparationTime = 10f },
            new RecipeResource { ItemName = "Pasta",   BaseQuality = 4f, Difficulty = 3f, BasePreparationTime = 25f },
            new RecipeResource { ItemName = "Pizza",   BaseQuality = 4f, Difficulty = 4f, BasePreparationTime = 30f },
            new RecipeResource { ItemName = "Steak",   BaseQuality = 5f, Difficulty = 5f, BasePreparationTime = 45f },
        };

        public RecipeResource GetRandomRecipe()
        {
            var pool = Recipes.Count > 0 ? Recipes : new Godot.Collections.Array<RecipeResource>(DefaultRecipes);
            return pool[(int)(GD.Randi() % (uint)pool.Count)];
        }
    }
}