using Godot;
using System;

public partial class Mapa : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public float gravedad =  ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * 4;

	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
