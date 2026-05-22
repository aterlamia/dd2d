using System;
using Godot;
using System.Collections.Generic;
using dd2d.restaurant.kitchen;
using dd2d.core;

namespace dd2d.restaurant
{
	public partial class VisitorManager : Node
	{
		[Export] public PackedScene VisitorScene { get; set; }
		[Export] public int MaxVisitors { get; set; } = 5;
		[Export] public float SpawnInterval { get; set; } = 10f;
		[Export] public int MaxPartySize { get; set; } = 2;
		[Export] public float MinPatience { get; set; } = 20f;
		[Export] public float MaxPatience { get; set; } = 60f;
		[Export] public float PartySpawnDelay { get; set; } = 0.8f;
		[Export] public Marker2D EntryPoint { get; set; }
		[Export] public core.Navigation.Navigator Navigator { get; set; }
		[Export] public kitchen.KitchenManager Kitchen { get; set; }
		[Export] public NodePath FurnitureLayerPath { get; set; }

		internal int _activeVisitors = 0;
		private Timer _spawnTimer;
		private readonly Random _random = new();
		private int _nextPartyId = 1;
		private readonly Dictionary<int, PartyState> _parties = new();

		private class PartyState
		{
			public int MemberCount;
			public int ReadyCount;
			public List<customer.Visitor> Members = new();
		}

		public override void _Ready()
		{
			if (VisitorScene == null)
			{
				Log.Error("VisitorScene is not set!", "VisitorManager");
				return;
			}
			if (EntryPoint == null)
			{
				Log.Error("EntryPoint is not set!", "VisitorManager");
				return;
			}
			if (Navigator == null)
			{
				Log.Error("Navigator is not set!", "VisitorManager");
				return;
			}
			if (FurnitureRegistry.Instance == null)
			{
				Log.Error("FurnitureRegistry not found!", "VisitorManager");
				return;
			}

			_spawnTimer = new Timer();
			_spawnTimer.WaitTime = SpawnInterval;
			_spawnTimer.Timeout += TrySpawnVisitor;
			AddChild(_spawnTimer);
			_spawnTimer.Start();

			TrySpawnVisitor();
		}

		internal void TrySpawnVisitor()
		{
			if (_activeVisitors >= MaxVisitors)
				return;

			var furnitureLayer = GetNodeOrNull<TileMapLayer>(FurnitureLayerPath);
			if (furnitureLayer == null)
			{
				GD.PrintErr("[VisitorManager] FurnitureLayer not found!");
				return;
			}

			int partySize = _random.Next(1, MaxPartySize + 1);

			if (partySize > 1)
			{
				TrySpawnParty(partySize, furnitureLayer);
				return;
			}

			TrySpawnSingle(furnitureLayer);
		}

		private bool TrySpawnParty(int partySize, TileMapLayer furnitureLayer)
		{
			foreach (var table in FurnitureRegistry.Instance.Tables)
			{
				if (table.IsOccupied || table.TotalSeats < partySize)
					continue;

				var freeChairs = new System.Collections.Generic.List<(table.ChairResource chair, Vector2 worldPos)>();
				foreach (var chair in table.Chairs)
				{
					if (!chair.IsOccupied)
					{
						Vector2I chairPos = table.Position + chair.ToOffset();
						freeChairs.Add((chair, furnitureLayer.MapToLocal(chairPos)));
					}
				}

				if (freeChairs.Count < partySize)
					continue;

				int partyId = _nextPartyId++;
				var party = new PartyState();
				_parties[partyId] = party;

				for (int i = 0; i < partySize; i++)
				{
					float delay = i * PartySpawnDelay;
					var chair = freeChairs[i].chair;
					var pos = freeChairs[i].worldPos;

					if (delay > 0)
					{
						var timer = GetTree().CreateTimer(delay);
						timer.Timeout += () => SpawnVisitor(chair, pos, "table", partyId);
					}
					else
					{
						SpawnVisitor(chair, pos, "table", partyId);
					}
				}

				return true;
			}

			return false;
		}

		private void TrySpawnSingle(TileMapLayer furnitureLayer)
		{
			var availableSeats = new System.Collections.Generic.List<(string source, table.ISeatingSpot seat, Vector2 worldPos)>();

			foreach (var (table, chair, worldPos) in FurnitureRegistry.Instance.GetAvailableTableSeats(furnitureLayer))
				availableSeats.Add(("table", chair, worldPos));

			foreach (var (stool, worldPos) in FurnitureRegistry.Instance.GetAvailableBarStools(furnitureLayer))
				availableSeats.Add(("bar", stool, worldPos));

			if (availableSeats.Count == 0)
			{
				Log.Info("No available seats.", "VisitorManager");
				return;
			}

			var (source, seat, destination) = availableSeats[_random.Next(availableSeats.Count)];
			SpawnVisitor(seat, destination, source);
		}

		private void SpawnVisitor(table.ISeatingSpot seat, Vector2 destination, string source, int partyId = -1)
		{
			var visitor = VisitorScene.Instantiate<customer.Visitor>();
			visitor.Navigator = Navigator;
			visitor.Kitchen = Kitchen;
			visitor.GlobalPosition = EntryPoint.GlobalPosition;
			visitor.Data = new customer.CustomerData();
			visitor.Data.Patience = (float)(_random.NextDouble() * (MaxPatience - MinPatience) + MinPatience);
			visitor.AssignedSeat = seat;

			if (partyId > 0)
			{
				visitor.PartyId = partyId;
				var capturedVisitor = visitor;
				visitor.OnPatienceExpired = () => OnPartyMemberPatienceExpired(capturedVisitor);

				if (_parties.TryGetValue(partyId, out var party))
				{
					party.Members.Add(visitor);
					party.MemberCount++;
				}
			}

			GetParent().AddChild(visitor);

			visitor.WalkToDestination(destination);
			var capturedSeat = seat;
			visitor.VisitCompleted += () => OnVisitorDone(visitor, capturedSeat);
			_activeVisitors++;
			Log.Info($"Spawned visitor ({_activeVisitors}/{MaxVisitors}) to {source} seat (patience {visitor.Data.Patience:F1}s)", "VisitorManager");
		}

		private void OnPartyMemberPatienceExpired(customer.Visitor visitor)
		{
			if (!_parties.TryGetValue(visitor.PartyId, out var party))
				return;

			party.ReadyCount++;
			Log.Debug($"Party {visitor.PartyId}: {party.ReadyCount}/{party.MemberCount} ready", "VisitorManager");

			if (party.ReadyCount >= party.MemberCount)
			{
				foreach (var member in party.Members)
					member.ContinueAfterWaiting();

				_parties.Remove(visitor.PartyId);
			}
		}

		private void OnVisitorDone(customer.Visitor visitor, table.ISeatingSpot seat)
		{
			_activeVisitors--;
			seat.IsOccupied = false;

			if (visitor.PartyId > 0 && _parties.TryGetValue(visitor.PartyId, out var party))
			{
				party.Members.Remove(visitor);
				if (party.Members.Count == 0)
					_parties.Remove(visitor.PartyId);
			}

			Log.Info($"Visitor despawned ({_activeVisitors}/{MaxVisitors})", "VisitorManager");
			visitor.QueueFree();
		}
	}
}