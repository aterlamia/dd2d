using System;
using Godot;
using dd2d.restaurant.customer;
using dd2d.restaurant;

namespace dd2d.core.StateMachine.Customer
{
	public partial class EatingState : Node
	{
		private Node2D _entity;
		private Action _onComplete;
		private Timer _eatTimer;

		public void Init(Node2D entity, float minEatTime, float maxEatTime, Action onComplete)
		{
			_entity = entity;
			_onComplete = onComplete;

			var visitor = entity as Visitor;
			if (visitor == null)
			{
				GD.PrintErr("[EatingState] Entity not Visitor");
				_onComplete?.Invoke();
				return;
			}

			float eatTime = (float)(new Random().NextDouble() * (maxEatTime - minEatTime) + minEatTime);
			Log.Debug($"Eating {eatTime:F1}s", "EatingState");

			_eatTimer = new Timer();
			_eatTimer.WaitTime = eatTime;
			_eatTimer.OneShot = true;
			_eatTimer.Timeout += OnEatFinished;
			AddChild(_eatTimer);
			_eatTimer.Start();
		}

		private void OnEatFinished()
		{
			var visitor = _entity as Visitor;
			if (visitor?.CurrentOrder != null)
			{
				var order = visitor.CurrentOrder;
				if (order.Recipe != null && VisitorManager.Instance != null)
				{
					VisitorManager.Instance.RegisterPayment(order.Recipe.Price);
					if (order.Result != null)
						VisitorManager.Instance.EmitSignal(VisitorManager.SignalName.RatingSubmitted, order.Result.FinalQuality, order.Recipe.ItemName);
				}
			}

			_onComplete?.Invoke();
			QueueFree();
		}

		public override void _ExitTree()
		{
			if (_eatTimer != null && IsInstanceValid(_eatTimer))
			{
				_eatTimer.Stop();
				_eatTimer.QueueFree();
			}
			base._ExitTree();
		}
	}
}
