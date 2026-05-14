using System;
using System.Linq;
using Godot;

namespace dd2d.restaurant.customer
{
    public partial class Visitor : Node2D
    {
        private float _speed = 60f;
        private Node _destinations;
        private Random _random;
        private core.Navigation.Navigator _navigator;
        private core.StateMachine.CustomerStateMachine _stateMachine;

        private Vector2 _actualDestination = Vector2.Zero;
        private Vector2 _navDestination = Vector2.Zero;
        private Vector2 _startPosition = Vector2.Zero;

        public override void _Ready()
        {
            _navigator = GetParent().GetNode<core.Navigation.Navigator>("Navigator");
            _destinations = GetParent().GetNode("Destinations");
            _random = new Random((int)DateTime.Now.Ticks);
            _startPosition = GlobalPosition;

            _stateMachine = new core.StateMachine.CustomerStateMachine();
            AddChild(_stateMachine);
            _stateMachine.Init(this);

            // Defer until all sibling nodes (including Navigator) have finished _Ready()
            CallDeferred(nameof(WalkToRandomDestination));
        }

        public void WalkTo(Vector2 targetPosition, Action onArrived = null)
        {
            _navDestination = targetPosition;
            var path = _navigator.GetPath(GlobalPosition, targetPosition);
            if (path.Length == 0)
                return;
            _stateMachine.StartWalking(_speed, path, onArrived);
        }

        public void WalkToRandomDestination()
        {
            if (_destinations == null || _destinations.GetChildCount() == 0)
                return;
            var markers = _destinations.GetChildren().OfType<Marker2D>().ToList();
            if (markers.Count == 0)
                return;
            markers = markers.OrderBy(_ => _random.Next()).ToList();
            int idx = _random.Next(markers.Count);
            _actualDestination = markers[idx].GlobalPosition;
            WalkTo(_actualDestination, OnArrivedAtDestination);
        }

        private void OnArrivedAtDestination()
        {
            GlobalPosition = _actualDestination;
            _stateMachine.StartSeated(10.0f, OnWaitFinished);
        }

        private void OnWaitFinished()
        {
            GlobalPosition = _navDestination;
            var path = _navigator.GetPath(GlobalPosition, _startPosition);
            _stateMachine.StartLeaving(_speed, path);
        }

        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("ui_select"))
                WalkToRandomDestination();
        }
    }
}
