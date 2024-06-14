using Godot;
using System;


public partial class Torretas : StaticBody3D
{
	MeshInstance3D cabeza;
	Node3D jugador;
	RayCast3D detector;
	Timer cronometro;
	Marker3D boca;
	OmniLight3D luz;
	AudioManager sfx; 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sfx = GetNode<AudioManager>("/root/AudioManager");
		luz = GetNode<OmniLight3D>("PuntoLuz");
		boca = GetNode<Marker3D>("Modelo/Cabeza/Boca");
		cronometro = GetNode<Timer>("Cronometro");
		detector = GetNode<RayCast3D>("DetectorTerreno");
		cabeza = GetNode<MeshInstance3D>("Modelo/Cabeza");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (jugador != null)
		{
			detector.TargetPosition = ToLocal(jugador.GlobalPosition);
		}
		else
		{
			detector.TargetPosition = new Vector3(0,0,0);
		}
		if (jugador != null && !detector.IsColliding())
		{
			cronometro.Paused = false;
			cabeza.LookAt(new Vector3(jugador.GlobalPosition.X, jugador.GlobalPosition.Y -2,jugador.GlobalPosition.Z));
			cabeza.Rotation = new Vector3(cabeza.Rotation.X , cabeza.Rotation.Y ,cabeza.Rotation.Z);
			luz.LightColor =  new Color(0.667f, 0, 0.137f);
		}
		else
		{

			luz.LightColor =  new Color(1, 1, 1);
			cronometro.Paused = true;
		}

	
	}

	private void AreaBody(Node3D body)
	{			
		cronometro.Start();
		if (body.IsInGroup("Jugador"))
		{
			jugador = body;
		}
	}
	Vector3 aplastadoEscala = new Vector3(1.8f,0.2f,1.8f);
	private async void Daño(String TipoAtaque, float DañoJugador)
	{
		Tween tween = GetTree().CreateTween();
			if (TipoAtaque == "AtaqueCaida")
			{	
				tween.TweenProperty(this, "scale", aplastadoEscala, 0.5 );
			}
			await ToSignal(tween,"finished"); //Lo mismo que await animation finished pero con tweens (animaciones a base de codigo)
			CallDeferred("queue_free");
	}
	private void AreaExit(Node3D body)
	{
		cronometro.Stop(); 
		if (body.IsInGroup("Jugador"))
		{
			jugador = null;
		}
	}
	PackedScene municion = GD.Load<PackedScene>("res://Escenas/Enemigo/Torretas/balas.tscn");
	private void disparar()
	{
		sfx.golpe.PitchScale = 0.1f;
		sfx.golpe.Play();
		var balas = (Bala)municion.Instantiate();
		balas.Position = boca.GlobalPosition;
		balas.Scale = new Vector3(0.2f,0.2f,0.2f);
		balas.jugadorPosition = jugador.GlobalPosition;
		
		GetParent().AddChild(balas);
	}

	public void Timeout()
	{
		disparar();
	}
}
