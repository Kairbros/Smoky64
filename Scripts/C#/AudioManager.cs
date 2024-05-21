using Godot;
using System;

public partial class AudioManager : Node
{
	public AudioStreamPlayer salto;
	public override void _Ready()
	{
		salto = GetNode<AudioStreamPlayer>("JumpSfx");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	
	}
}
