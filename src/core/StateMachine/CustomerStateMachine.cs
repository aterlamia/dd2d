using System;
using System.Collections.Generic;
using Godot;
using dd2d.restaurant.table;
using dd2d.restaurant.customer;
using dd2d.core;

namespace dd2d.core.StateMachine
{
	public enum CustomerStateType { Idle, Waiting, Walking, Seated, StandingUp, Leaving, Eating }

	public partial class CustomerStateMachine : StateMachine
	{
		public CustomerStateType CurrentState { get; private set; } = CustomerStateType.Idle;

		private Node2D _entity;
		private Node _activeState;
		private readonly Queue<Action> _sequence = new();
		private ISeatingSpot _assignedSeat;
		private float _speed;
		private Vector2[] _returnPath;
		private Vector2 _seatPosition;
		private bool _wasServed = false;
		private Action _onComplete;

		public void Init(Node2D entity)
		{
			_entity = entity;
			Log.Debug("Initialized", "CustomerStateMachine");
		}

		// Queues up the full visit sequence and starts it.
		// Cancel at any time with CancelSequence().
	public void BeginVisit(float speed, Vector2[] walkPath, Vector2 seatPosition, float patience, Vector2[] returnPath, ISeatingSpot seat, Action onComplete = null, Action onPatienceExpired = null, Action onSeated = null)
	{
		Log.Debug("BeginVisit started", "CustomerStateMachine");
		_assignedSeat = seat;
		_speed = speed;
		_seatPosition = seatPosition;
		_returnPath = returnPath;
		_onComplete = onComplete;
		_wasServed = false;
		_sequence.Clear();

			Action patienceCallback = () =>
			{
				if (_wasServed)
					return;
				(onPatienceExpired ?? AdvanceQueue)();
			};

			_sequence.Enqueue(() => StartWalking(speed, walkPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				_entity.GlobalPosition = seatPosition;
				StartSeated(() =>
				{
					onSeated?.Invoke();
					AdvanceQueue();
				});
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

		public void StartEating()
		{
			var visitor = _entity as Visitor;
			if (visitor == null || visitor.Data == null)
			{
				AdvanceQueue();
				return;
			}

			float min = visitor.Data.MinEatTime;
			float max = visitor.Data.MaxEatTime;
			Log.Debug($"{CurrentState} → Eating (min {min}s, max {max}s)", "CustomerStateMachine");
			ClearActiveState();
			CurrentState = CustomerStateType.Eating;
			var state = new Customer.EatingState();
			_activeState = state;
			AddChild(state);
			state.Init(_entity, min, max, AdvanceQueue);
		}

		public void ProceedFromWaiting()
		{
			if (_activeState is Customer.WaitingState waiting)
			{
				waiting.StopAndHide();
			}

			_sequence.Clear();
			_sequence.Enqueue(() =>
			{
				Vector2 stepTo = _returnPath.Length > 0 ? _returnPath[0] : _seatPosition;
				StartStandingUp(stepTo, AdvanceQueue);
			});
			_sequence.Enqueue(() => StartLeaving(_speed, _returnPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				Log.Info("Visit sequence complete", "CustomerStateMachine");
				_assignedSeat = null;
				_onComplete?.Invoke();
			});
			AdvanceQueue();
		}

		public void FoodArrived(float patienceBonus)
		{
			if (_activeState is not Customer.WaitingState waiting)
				return;

			_wasServed = true;
			waiting.AddPatience(patienceBonus);
			waiting.StopAndHide();

			_sequence.Clear();
			_sequence.Enqueue(() => StartEating());
			_sequence.Enqueue(() =>
			{
				Vector2 stepTo = _returnPath.Length > 0 ? _returnPath[0] : _seatPosition;
				StartStandingUp(stepTo, AdvanceQueue);
			});
			_sequence.Enqueue(() => StartLeaving(_speed, _returnPath, AdvanceQueue));
			_sequence.Enqueue(() =>
			{
				Log.Info("Visit sequence complete", "CustomerStateMachine");
				_assignedSeat = null;
				_onComplete?.Invoke();
			});
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
