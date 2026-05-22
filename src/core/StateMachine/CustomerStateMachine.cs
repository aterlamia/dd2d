using System;
using System.Collections.Generic;
using Godot;
using dd2d.restaurant.table;
using dd2d.core;

namespace dd2d.core.StateMachine
{
	public enum CustomerStateType { Idle, Waiting, Walking, Seated, StandingUp, Leaving }

	public partial class CustomerStateMachine : StateMachine
	{
		public CustomerStateType CurrentState { get; private set; } = CustomerStateType.Idle;

		private Node2D _entity;
		private Node _activeState;
		private readonly Queue<Action> _sequence = new();
		private ISeatingSpot _assignedSeat;

		public void Init(Node2D entity)
		{
			_entity = entity;
			Log.Debug("Initialized", "CustomerStateMachine");
		}

		// Queues up the full visit sequence and starts it.
		// Cancel at any time with CancelSequence().
		public void BeginVisit(float speed, Vector2[] walkPath, Vector2 seatPosition, float patience, Vector2[] returnPath, ISeatingSpot seat, Action onComplete = null, Action onPatienceExpired = null)
		{
			Log.Debug("BeginVisit started", "CustomerStateMachine");
			_assignedSeat = seat;
			_sequence.Clear();

			Action patienceCallback = onPatienceExpired ?? AdvanceQueue;

			_sequence.Enqueue(() => StartWalking(speed, walkPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				_entity.GlobalPosition = seatPosition;
				StartSeated(AdvanceQueue);
			});
			_sequence.Enqueue(() => StartWaiting(patience, patienceCallback));
			_sequence.Enqueue(() =>
			{
				Vector2 stepTo = returnPath.Length > 0 ? returnPath[0] : seatPosition;
				StartStandingUp(stepTo, AdvanceQueue);
			});
			_sequence.Enqueue(() => StartLeaving(speed, returnPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				Log.Info("Visit sequence complete", "CustomerStateMachine");
				_assignedSeat = null;
				onComplete?.Invoke();
			});

			AdvanceQueue();
		}

	// Stops the current sequence and clears pending steps.
	public void CancelSequence()
	{
		Log.Info("Sequence cancelled", "CustomerStateMachine");
		_sequence.Clear();
		ClearActiveState();
	}

		public void StartWalking(float speed, Vector2[] path, Action onArrived)
		{
			Log.Debug($"{CurrentState} → Walking", "CustomerStateMachine");
			ClearActiveState();
			CurrentState = CustomerStateType.Walking;
			var state = new Customer.WalkingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, speed, path, onArrived);
		}

		public void StartSeated(Action onSeated)
		{
			Log.Debug($"{CurrentState} → Seated", "CustomerStateMachine");
			ClearActiveState();
			CurrentState = CustomerStateType.Seated;
			var state = new Customer.SeatedState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, _assignedSeat, onSeated);
		}

		public void StartWaiting(float patience, Action onPatienceExpired)
		{
			Log.Debug($"{CurrentState} → Waiting (patience {patience}s)", "CustomerStateMachine");
			ClearActiveState();
			CurrentState = CustomerStateType.Waiting;
			var state = new Customer.WaitingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, patience, onPatienceExpired);
		}

		public void StartStandingUp(Vector2 stepToPosition, Action onStoodUp)
		{
			Log.Debug($"{CurrentState} → StandingUp", "CustomerStateMachine");
			ClearActiveState();
			CurrentState = CustomerStateType.StandingUp;
			var state = new Customer.StandingUpState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, stepToPosition, _assignedSeat, onStoodUp);
		}

		public void StartLeaving(float speed, Vector2[] path, Action onArrived = null)
		{
			Log.Debug($"{CurrentState} → Leaving", "CustomerStateMachine");
			ClearActiveState();
			CurrentState = CustomerStateType.Leaving;
			var state = new Customer.LeavingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, speed, path, onArrived);
		}

		public void ProceedFromWaiting()
		{
			AdvanceQueue();
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
