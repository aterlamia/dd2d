using Godot;

namespace dd2d.restaurant.table
{
    public partial class Chair : Node2D
    {
        [Export]
        public bool IsOccupied { get; private set; } = false;

        public void Occupy()
        {
            IsOccupied = true;
        }

        public void Vacate()
        {
            IsOccupied = false;
        }
    }
}

