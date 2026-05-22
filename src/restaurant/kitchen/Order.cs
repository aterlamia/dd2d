using Godot;

namespace dd2d.restaurant.kitchen
{
    public partial class Order : RefCounted
    {
        public RecipeResource Recipe { get; set; }
        public Node2D Customer { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Chef AssignedChef { get; set; }
        public CookedItem Result { get; set; }
        public float PlacedTime { get; set; }
    }
}