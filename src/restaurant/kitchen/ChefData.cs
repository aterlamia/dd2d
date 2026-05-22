using Godot;

namespace dd2d.restaurant.kitchen
{
    [GlobalClass]
    public partial class ChefData : Resource
    {
        [Export] public string ChefName { get; set; }
        [Export] public float Speed { get; set; } = 1f;
        [Export] public float Presentation { get; set; } = 1f;
        [Export] public float Experience { get; set; } = 1f;
    }
}