using System;
using Godot;
using dd2d.core;

namespace dd2d.core.StateMachine.Customer
{
	public partial class WaitingState : Node
	{
		private Node2D _entity;
		private float _patience;
		private Action _onPatienceExpired;
		private Timer _patienceTimer;
		private TextureProgressBar _indicator;

		public void Init(Node2D entity, float patience, Action onPatienceExpired)
		{
			_entity = entity;
			_patience = patience;
			_onPatienceExpired = onPatienceExpired;

			_patienceTimer = new Timer();
			_patienceTimer.WaitTime = _patience;
			_patienceTimer.OneShot = true;
			_patienceTimer.Timeout += OnPatienceExpired;
			AddChild(_patienceTimer);
			_patienceTimer.Start();

			_indicator = entity.GetNodeOrNull<TextureProgressBar>("Indicator");
			if (_indicator != null)
			{
				_indicator.MinValue = 0;
				_indicator.MaxValue = _patience;
				_indicator.Value = _patience;
				_indicator.TintProgress = Colors.Green;
				_indicator.Visible = true;
			}

			entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer")?.Play(core.AnimationKeys.Idle);
		}

		public override void _Process(double delta)
		{
			if (_indicator == null) return;
			float remaining = (float)_patienceTimer.TimeLeft;
			_indicator.Value = remaining;
			float ratio = remaining / _patience;
			_indicator.TintProgress = Colors.Red.Lerp(Colors.Green, ratio);
		}

		public float GetRemainingPatience() => (float)_patienceTimer.TimeLeft;

		public void AddPatience(float extraTime)
		{
			float remaining = (float)_patienceTimer.TimeLeft;
			_patience += extraTime;
			_patienceTimer.Start(remaining + extraTime);
			if (_indicator != null)
			{
				_indicator.MaxValue = _patience;
				_indicator.Value = remaining + extraTime;
			}
		}

		private void OnPatienceExpired()
		{
			Log.Info("Patience expired!", "WaitingState");
			if (_indicator != null)
				_indicator.Visible = false;
			_onPatienceExpired?.Invoke();
			QueueFree();
		}
	}
}
