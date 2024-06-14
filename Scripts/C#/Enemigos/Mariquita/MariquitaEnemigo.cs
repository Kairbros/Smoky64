using Godot;
using System;

public partial class MariquitaEnemigo : CharacterBody3D
{
	EnemigoPadre enemigo;
	[Export] float vida = 1;
	[Export] float velocidad = 15;
	[Export] string tipoEnemigo = "Mariquita";
	float gravedad;
	
	Vector3 aplastadoEscala = new Vector3(1.8f,0.2f,1.8f);
	Boolean golpeado; 
	Boolean visto;

	Mapa mapa;
	Node3D jugador;
	RayCast3D detectorTerreno;
	NavigationAgent3D navegacion;
 	AnimationPlayer animacion;
	AnimationPlayer animacionEfectos;
	CollisionShape3D colisionAtaque;
 	CollisionShape3D colision;
	Timer cronometro;
	TextureProgressBar barraVida;

	public override void _Ready()
	{
		animacionEfectos = GetNode<AnimationPlayer>("Personaje/AnimacionEfectos");
		enemigo = new EnemigoPadre();
		barraVida = GetNode<TextureProgressBar>("BarraVida/SubViewport/BarraVida");
		tipoEnemigo = "Mariquita";
		navegacion = GetNode<NavigationAgent3D>("Navegacion");
		detectorTerreno = GetNode<RayCast3D>("ParedDetector");
		cronometro = GetNode<Timer>("Cronometro");
		colision = GetNode<CollisionShape3D>("Colision");
		colisionAtaque = GetNode<CollisionShape3D>("AreaAtaque/Colision");
		animacion = GetNode<AnimationPlayer>("Personaje/Animacion");
		mapa = GetParent<Mapa>();
		gravedad = mapa.gravedad;
		barraVida.MaxValue = vida;


	}
	public override void _Process(double delta)
	{

		_vision();
		movimiento(delta);
		estadoSalud();
		colisiones();
		animaciones();
		MoveAndSlide();
	}
	public void _vision()
	{
		if (jugador != null)
		{
			detectorTerreno.TargetPosition = ToLocal(jugador.GlobalPosition);
			if (!detectorTerreno.IsColliding())
			{
				visto =  true;
			}
		}
		else
		{
			detectorTerreno.TargetPosition = new Vector3(0,0,0);
			visto = false;
		}
	}
	private void animaciones()
	{
		enemigo._animaciones(animacion,Velocity, animacionEfectos, golpeado);
	}
	private void movimiento(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity = enemigo._gravedad(delta, gravedad, Velocity);
		}
		if (jugador != null)
		{
			if (cronometro.TimeLeft <= 0)
			{
			if (visto)
			{
				LookAt(navegacion.GetNextPathPosition());
				Rotation = new Vector3(0, Rotation.Y,0);
				Velocity = enemigo._movimiento(Velocity,GlobalPosition,navegacion.GetNextPathPosition(),velocidad);
			}
			}
			else
			{
				Velocity = new Vector3(0, Velocity.Y, 0);
			}
		}
		else
			{
				Velocity = new Vector3(0, Velocity.Y, 0);
			}
	}	
	private void colisiones()
	{
		colisionAtaque.Disabled = enemigo._colisiones(cronometro);
	}
	private void ubicacionJugador(Vector3 ubicacionJugador)
	{
		navegacion.TargetPosition =  ubicacionJugador;
	}
	private void AreaAtaque (Node3D body)
	{
		if (body.IsInGroup("Jugador")) //  compara si el elemento que entra es de una clase en especifico
		{
			cronometro.WaitTime = 1.5;
			cronometro.Start();
			body.Call("Daño", enemigo._empujeEnemigo(body.GlobalPosition,GlobalPosition,velocidad), tipoEnemigo, velocidad);
		}
	}
	private void AreaBody(Node3D body)
	{
		if (body.IsInGroup("Jugador"))
		{
			jugador = body;
		}
	}
	private void AreaBodyExit(Node3D body)
	{
		if (body.IsInGroup("Jugador"))
		{
			jugador = null;
		}
	}
	private void Daño(String TipoAtaque, float DañoJugador)
	{
		golpeado = true;
		vida = vida - DañoJugador;
		cronometro.WaitTime = 1.5f;
		cronometro.Start();
		muerte(TipoAtaque);
	}
	private async void estadoSalud()
	{
		barraVida.Value = vida;
		if (golpeado)
		{
			await ToSignal(cronometro,"timeout");
			golpeado = false;
		}
	}
	private async void muerte(string TipoAtaque)
	{
		if (enemigo._muerte(vida))
		{	
			Tween tween = GetTree().CreateTween();
			if (TipoAtaque == "AtaqueCaida")
			{
				colisionAtaque.Disabled = true;
				tween.TweenProperty(this, "scale", aplastadoEscala, 0.5 );
			}
			await ToSignal(tween,"finished"); //Lo mismo que await animation finished pero con tweens (animaciones a base de codigo)
			QueueFree();
		}
	}
}
