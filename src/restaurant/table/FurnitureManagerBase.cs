using Godot;
using dd2d.core;

namespace dd2d.restaurant.table
{
    public abstract partial class FurnitureManagerBase : Node
    {
        [Export]
        public NodePath FurnitureLayerPath { get; set; }

        [Export]
        public string FurnitureJsonPath { get; set; } = "res://data/furniture.json";

        public override void _Ready()
        {
            var furnitureLayer = GetNodeOrNull<TileMapLayer>(FurnitureLayerPath);
            if (furnitureLayer == null)
            {
                Log.Error("FurnitureLayer not found", GetType().Name);
                return;
            }

            if (FurnitureRegistry.Instance == null)
            {
                Log.Error("FurnitureRegistry not initialized", GetType().Name);
                return;
            }

            var root = ModLoader.LoadMergedJson(FurnitureJsonPath);
            if (root.Count == 0)
            {
                Log.Error($"Failed to load JSON: {FurnitureJsonPath}", GetType().Name);
                return;
            }

            LoadFurniture(root, furnitureLayer);
        }

        protected abstract void LoadFurniture(Godot.Collections.Dictionary root, TileMapLayer furnitureLayer);
    }
}
