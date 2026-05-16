using System;
using System.Collections.Generic;
using Godot;

namespace dd2d.core.StateMachine
{
	public enum CustomerStateType { Idle, Waiting, Walking, Seated, StandingUp, Leaving }

	public partial class CustomerStateMachine : StateMachine
	{
		public CustomerStateType CurrentState { get; private set; } = CustomerStateType.Idle;

		private Node2D _entity;
		private Node _activeState;
		private readonly Queue<Action> _sequence = new();

		public void Init(Node2D entity)
		{
			_entity = entity;
			GD.Print("[CustomerStateMachine] Initialized");
		}

		// Queues up the full visit sequence and starts it.
		// Cancel at any time with CancelSequence().
		public void BeginVisit(float speed, Vector2[] walkPath, Vector2 seatPosition, float patience, Vector2[] returnPath, Action onComplete = null)
		{
			GD.Print("[CustomerStateMachine] BeginVisit started");
			_sequence.Clear();

			_sequence.Enqueue(() => StartWalking(speed, walkPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				_entity.GlobalPosition = seatPosition;
				StartSeated(AdvanceQueue);
			});
			_sequence.Enqueue(() => StartWaiting(patience, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				Vector2 stepTo = returnPath.Length > 0 ? returnPath[0] : seatPosition;
				StartStandingUp(stepTo, AdvanceQueue);
			});
			_sequence.Enqueue(() => StartLeaving(speed, returnPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				GD.Print("[CustomerStateMachine] Visit sequence complete");
				onComplete?.Invoke();
			});

			AdvanceQueue();
		}

		// Stops the current sequence and clears pending steps.
		public void CancelSequence()
		{
			GD.Print("[CustomerStateMachine] Sequence cancelled");
			_sequence.Clear();
			ClearActiveState();
		}

		public void StartWalking(float speed, Vector2[] path, Action onArrived)
		{
			GD.Print($"[CustomerStateMachine] {CurrentState} → Walking");
			ClearActiveState();
			CurrentState = CustomerStateType.Walking;
			var state = new Customer.WalkingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, speed, path, onArrived);
		}

		public void StartSeated(Action onSeated)
		{
			GD.Print($"[CustomerStateMachine] {CurrentState} → Seated");
			ClearActiveState();
			CurrentState = CustomerStateType.Seated;
			var state = new Customer.SeatedState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, onSeated);
		}

		public void StartWaiting(float patience, Action onPatienceExpired)
		{
			GD.Print($"[CustomerStateMachine] {CurrentState} → Waiting (patience {patience}s)");
			ClearActiveState();
			CurrentState = CustomerStateType.Waiting;
			var state = new Customer.WaitingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, patience, onPatienceExpired);
		}

		public void StartStandingUp(Vector2 stepToPosition, Action onStoodUp)
		{
			GD.Print($"[CustomerStateMachine] {CurrentState} → StandingUp");
			ClearActiveState();
			CurrentState = CustomerStateType.StandingUp;
			var state = new Customer.StandingUpState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, stepToPosition, onStoodUp);
		}

		public void StartLeaving(float speed, Vector2[] path, Action onArrived = null)
		{
			GD.Print($"[CustomerStateMachine] {CurrentState} → Leaving");
			ClearActiveState();
			CurrentState = CustomerStateType.Leaving;
			var state = new Customer.LeavingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, speed, path, onArrived);
		}

		private void AdvanceQueue()
		{
			if (_sequence.Count == 0) return;
			_sequence.Dequeue().Invoke();
		}

		private void ClearActiveState()
		{
			if (_activeState != null && IsInstanceValid(_activeState))
			{
				_activeState.QueueFree();
				_activeState = null;
			}
			CurrentState = CustomerStateType.Idle;
		}
	}
}
