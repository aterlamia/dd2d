using Godot;
using dd2d.core;

namespace dd2d.restaurant.kitchen
{
    [GlobalClass]
    public partial class RecipeResource : Resource
    {
        [Export] public string ItemName { get; set; }
        [Export] public float BaseQuality { get; set; } = 3f;
        [Export] public float Difficulty { get; set; } = 3f;
        [Export] public float BasePreparationTime { get; set; } = 20f;

        public static RecipeResource FromDictionary(Godot.Collections.Dictionary dict)
        {
            if (!dict.ContainsKey("name"))
            {
                Log.Error("Recipe JSON missing 'name' key", "RecipeResource");
                return null;
            }

            var recipe = new RecipeResource();
            recipe.ItemName = dict["name"].AsString();

            recipe.BaseQuality = dict.ContainsKey("base_quality")
                ? dict["base_quality"].AsSingle()
                : 3f;

            recipe.Difficulty = dict.ContainsKey("difficulty")
                ? dict["difficulty"].AsSingle()
                : 3f;

            recipe.BasePreparationTime = dict.ContainsKey("preparation_time")
                ? dict["preparation_time"].AsSingle()
                : 20f;

            return recipe;
        }
    }
}