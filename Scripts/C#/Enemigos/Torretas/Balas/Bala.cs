using Godot;
using System;




public partial class Bala : RigidBody3D
{
	public Vector3 jugadorPosition;
	public Vector3 direccion;

	float velocidad =  700;

	CollisionShape3D AreaDaño;

	public override void _Ready()
	{
		AreaDaño = GetNode<CollisionShape3D>("AreaDaño/Colision");
		direccion = (jugadorPosition - this.GlobalPosition).Normalized();
	}


	public override void _Process(double delta)
	{
		Vector3 movimiento = direccion * velocidad * (float)delta;
		ApplyImpulse(movimiento);
	}
	
	private void AreaBody(Node3D body)
	{
		AreaDaño.Disabled = true;
		if (body.IsInGroup("Jugador"))
		{
			Vector3 empuje = body.GlobalPosition - GlobalPosition;
			empuje.Normalized();

			body.Call("Daño", -empuje * 10, "Bala", 10);
			 CallDeferred("queue_free");
		}
	}
	private void Timeout()
	{
		 CallDeferred("queue_free");
	}
	
}
