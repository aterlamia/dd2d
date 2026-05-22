using Godot;
using System.Collections.Generic;
using dd2d.core;

namespace dd2d.restaurant.kitchen
{
    public partial class KitchenManager : Node
    {
        [Signal]
        public delegate void OrderCompletedEventHandler(Order order);

        [Export]
        public MenuManager Menu { get; set; }

        [Export]
        public Godot.Collections.Array<NodePath> ChefPaths { get; set; } = new();

        [Export]
        public string RecipesJsonPath { get; set; } = "res://data/recipes.json";

        private readonly Queue<Order> _orderQueue = new();
        private readonly List<Chef> _chefs = new();

        private int _totalOrdersPlaced = 0;
        private int _totalOrdersCompleted = 0;

        public override void _Ready()
        {
            foreach (var path in ChefPaths)
            {
                var chef = GetNodeOrNull<Chef>(path);
                if (chef != null)
                {
                    _chefs.Add(chef);
                    chef.CookingFinished += OnChefFinished;
                }
            }

            if (Menu == null)
                Menu = new MenuManager();

            LoadRecipes();

            Log.Debug($"KitchenManager ready with {_chefs.Count} chef(s) and {Menu.Recipes.Count} recipe(s)", "KitchenManager");
        }

        private void LoadRecipes()
        {
            var root = ModLoader.LoadMergedJson(RecipesJsonPath);
            if (root.Count == 0 || !root.ContainsKey("recipes"))
            {
                Log.Debug($"No recipes JSON found at {RecipesJsonPath}, using defaults", "KitchenManager");
                return;
            }

            var recipesArr = root["recipes"].AsGodotArray();
            foreach (var entry in recipesArr)
            {
                var dict = entry.AsGodotDictionary();
                var recipe = RecipeResource.FromDictionary(dict);
                if (recipe != null)
                    Menu.Recipes.Add(recipe);
            }

            if (Menu.Recipes.Count == 0)
                Log.Error("No valid recipes loaded from JSON", "KitchenManager");
        }

        public Order PlaceOrder(Node2D customer)
        {
            var recipe = Menu.GetRandomRecipe();
            var order = new Order
            {
                Recipe = recipe,
                Customer = customer,
                Status = OrderStatus.Pending,
                PlacedTime = Time.GetTicksMsec() / 1000f
            };

            _orderQueue.Enqueue(order);
            _totalOrdersPlaced++;
            Log.Debug($"Order placed: {recipe.ItemName} for {customer.Name} (queue: {_orderQueue.Count})", "KitchenManager");

            TryAssignOrders();
            return order;
        }

        private void TryAssignOrders()
        {
            while (_orderQueue.Count > 0)
            {
                var idleChef = _chefs.Find(c => c.IsIdle);
                if (idleChef == null) break;

                var order = _orderQueue.Dequeue();
                idleChef.StartCooking(order);
            }
        }

        private void OnChefFinished(CookedItem item)
        {
            _totalOrdersCompleted++;

            Order completedOrder = null;
            foreach (var chef in _chefs)
            {
                if (chef.CurrentOrder?.Result == item)
                {
                    completedOrder = chef.CurrentOrder;
                    break;
                }
            }

            if (completedOrder != null)
            {
                Log.Debug($"Order completed: {item.Recipe.ItemName} (quality {item.FinalQuality:F1})", "KitchenManager");
                EmitSignal(SignalName.OrderCompleted, completedOrder);
            }

            TryAssignOrders();
        }

        public int PendingOrderCount => _orderQueue.Count;
        public int ActiveOrdersCount => _chefs.FindAll(c => !c.IsIdle).Count;
    }
}