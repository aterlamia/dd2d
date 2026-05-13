using Godot;
using System;

public partial class Restaurant : Node2D
{
	public override void _Ready()
	{
		Rid defaultNavigationMapRid = GetWorld2D().NavigationMap;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
