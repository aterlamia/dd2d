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
		private bool _stopped = false;

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
				// UI visibility managed by Visitor or specific events
			}

			entity.GetNodeOrNull<AnimationPlayer>("AnimationPlayer")?.Play(core.AnimationKeys.Idle);
		}

		public override void _Process(double delta)
		{
			if (_stopped) return;
			if (!GodotObject.IsInstanceValid(this)) return;
			if (_patienceTimer == null || !GodotObject.IsInstanceValid(_patienceTimer)) return;
			if (_indicator == null || !GodotObject.IsInstanceValid(_indicator)) return;
			float remaining = (float)_patienceTimer.TimeLeft;
			_indicator.Value = remaining;
			float ratio = remaining / _patience;
			_indicator.TintProgress = Colors.Red.Lerp(Colors.Green, ratio);
		}

		public float GetRemainingPatience() => (float)_patienceTimer.TimeLeft;

		public void StopAndHide()
		{
			_stopped = true;
			SetProcess(false);
			if (_patienceTimer != null)
			{
				_patienceTimer.Timeout -= OnPatienceExpired;
				_patienceTimer.Stop();
				_patienceTimer = null;
			}
			if (_indicator != null)
				_indicator.Visible = false;
			
			var icon = _entity.GetNodeOrNull<CanvasItem>("Icon");
			if (icon != null)
				icon.Visible = false;
		}

		public void AddPatience(float extraTime)
		{
			if (_stopped || _patienceTimer == null)
				return;
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
			if (_stopped)
				return;
			_stopped = true;
			SetProcess(false);
			Log.Info("Patience expired!", "WaitingState");
			if (_patienceTimer != null)
			{
				_patienceTimer.Timeout -= OnPatienceExpired;
				_patienceTimer = null;
			}
			if (_indicator != null)
				_indicator.Visible = false;
			
			var icon = _entity.GetNodeOrNull<CanvasItem>("Icon");
			if (icon != null)
				icon.Visible = false;

			_onPatienceExpired?.Invoke();
			QueueFree();
		}
	}
}
