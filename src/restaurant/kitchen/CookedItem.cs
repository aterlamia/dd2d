using Godot;

namespace dd2d.restaurant.kitchen
{
    [GlobalClass]
    public partial class CookedItem : Resource
    {
        [Export] public RecipeResource Recipe { get; set; }
        [Export] public float FinalQuality { get; set; }
        [Export] public ChefData CookedBy { get; set; }
        [Export] public float ActualPreparationTime { get; set; }

        public static CookedItem Cook(RecipeResource recipe, ChefData chef)
        {
            float speedMultiplier = chef.Speed > 0 ? 1f / chef.Speed : 1f;
            float actualTime = recipe.BasePreparationTime * speedMultiplier;

            float skillGap = chef.Experience - recipe.Difficulty;
            float variance = (GD.Randf() - 0.5f) * 0.4f;
            float finalQuality = Mathf.Clamp(
                recipe.BaseQuality + skillGap * 0.25f + chef.Presentation * 0.1f + variance,
                1f, 5f);

            return new CookedItem
            {
                Recipe = recipe,
                FinalQuality = Mathf.Round(finalQuality * 10f) / 10f,
                CookedBy = chef,
                ActualPreparationTime = actualTime
            };
        }
    }
}