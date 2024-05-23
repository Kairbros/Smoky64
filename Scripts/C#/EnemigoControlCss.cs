using Godot;
using System;

public partial class EnemigoControlCss : RigidBody3D
{

	public Mapa mapa;
	public RayCast3D DetectorPared;
	public Node3D jugador;
	public Node3D modelo;
	public float temporizadorAtaque = 0;
	public float temporizadorCd = 0;
	public override void _Ready()
	{
		mapa = GetParent<Mapa>();
		DetectorPared = GetNode<RayCast3D>("ParedDetector");
		modelo =  GetNode<Node3D>("Personaje/Modelo");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_detector(delta);
		_movimientos(delta);
	}

	public void _movimientos(double delta)
	{
		if (jugador != null)
		{

			GlobalPosition = GlobalPosition.MoveToward(new Vector3(jugador.GlobalPosition.X,  GlobalPosition.Y  ,jugador.GlobalPosition.Z), 7 * (float)delta);
			
		
		}
	}
	public void _detector(double delta)
	{
		if (jugador != null)
			{
			
				DetectorPared.TargetPosition = ToLocal(jugador.GlobalPosition);
				LookAt(new Vector3(jugador.GlobalPosition.X, GlobalTransform.Origin.Y , jugador.GlobalPosition.Z));
				
			
			}	
	}
	public void OnAreaBodyEntered(Node3D body){

		if (body.IsInGroup("Jugador"))
		{
		jugador = body;
		}
	}
	
	public void OnAreaBodyExit(Node3D _body){
		jugador = null;
	}

	public void OnAreaAtaqueEntered(Node3D body){
		if (body is PlayerCs)
		{
			body.Call("Daño","Bonais",new Vector3(0,15,0));
		}
	}
}
