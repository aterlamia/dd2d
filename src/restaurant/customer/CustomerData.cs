using Godot;

namespace dd2d.restaurant.customer;

[GlobalClass]
public partial class CustomerData : Resource
{
    [Export] public string Name { get; set; }
    [Export] public float Speed { get; set; } = 60f;
    [Export] public float Patience { get; set; } = 60f; // seconds
    [Export] public float MinEatTime { get; set; } = 30f;
    [Export] public float MaxEatTime { get; set; } = 90f;
    [Export] public Color PreferenceColor { get; set; }
    [Export] public bool IsRegular { get; set; } = false;
    [Export] public Texture2D Sprite { get; set; }
}

