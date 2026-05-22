using Godot;
using System;
using dd2d.core;

namespace dd2d.restaurant.kitchen
{
    public partial class Chef : Node2D
    {
        [Signal]
        public delegate void CookingFinishedEventHandler(CookedItem item);

        [Export]
        public ChefData ChefData { get; set; }

        public bool IsIdle { get; private set; } = true;
        public Order CurrentOrder { get; private set; }

        private Timer _cookTimer;
        private Sprite2D _sprite;

        public override void _Ready()
        {
            _sprite = new Sprite2D();
            AddChild(_sprite);

            _cookTimer = new Timer();
            _cookTimer.OneShot = true;
            _cookTimer.Timeout += OnCookingDone;
            AddChild(_cookTimer);
        }

        public void StartCooking(Order order)
        {
            if (!IsIdle || order == null) return;

            IsIdle = false;
            CurrentOrder = order;
            order.Status = OrderStatus.InProgress;
            order.AssignedChef = this;

            float cookTime = order.Recipe.BasePreparationTime / ChefData.Speed;
            Log.Debug($"Chef {ChefData.ChefName} cooking {order.Recipe.ItemName} ({cookTime:F1}s)", "Chef");
            _cookTimer.WaitTime = cookTime;
            _cookTimer.Start();
        }

        private void OnCookingDone()
        {
            var item = CookedItem.Cook(CurrentOrder.Recipe, ChefData);
            CurrentOrder.Result = item;
            CurrentOrder.Status = OrderStatus.Completed;

Log.Debug($"Chef {ChefData.ChefName} finished {item.Recipe.ItemName} (quality {item.FinalQuality:F1})", "Chef");

			EmitSignal(SignalName.CookingFinished, item);
			CurrentOrder = null;
			IsIdle = true;
        }
    }
}