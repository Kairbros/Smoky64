using Godot;
using System;

public partial class Jugador : CharacterBody3D
{	
	JugadorPadre jugador;

	Boolean sePuedeDobleSalto;
	Boolean dobleSalto;
	Boolean sprint;
	Boolean sePuedeDesplazamiento;
	Boolean desplazamiento;
	Boolean golpe;
	Boolean atacando;
	
	const float GirarPersonaje = 12;

	const String ataquCaida = "AtaqueCaida";
	const String ataqueCola = "AtaqueCola";

	[Export] float velocidadBase = 7;
	[Export] float velocidadCorrer = 14;
	[Export] float velocidadDesplazamiento = 25;
	[Export] float velocidadCaidaAtaque = 35;
	[Export] float velocidadRebote = 10;

	[Export] float fuerzaSalto = 10;
	[Export] float dañoAtaqueCaida = 1;
	[Export] float vida = 3;

	float velocidad = 0;

	float friccion = 10;
	float friccionAire = 10;

	float fuerzaGravedad;

	Mapa mapa;

	Vector3 direccion;
	Vector3 direccionDesplazamiento = new Vector3(0,0,1);
	
	CollisionShape3D colision;
	Node3D camara;
	AnimationPlayer animacionModelo;
	AnimationPlayer animacionCara;
	AnimationPlayer animacionEfectos;
	Timer cronometro;
	CollisionShape3D colisionAtaque;
	AudioStreamPlayer3D sfxPasos;
	AudioManager sfx; 
	CpuParticles3D particulasCaminar;
	CpuParticles3D particulasAtaque;

	public override void _Ready()
	{
		mapa = GetParent<Mapa>();  // Get_parent Variable

		jugador = new JugadorPadre();
		fuerzaGravedad = mapa.gravedad;


		particulasCaminar = GetNode<CpuParticles3D>("ParticulasCaminar");
		particulasAtaque = GetNode<CpuParticles3D>("ParticulasAtaque");  
		sfx = GetNode<AudioManager>("/root/AudioManager");
		sfxPasos = GetNode<AudioStreamPlayer3D>("Personaje/Pasos");
		colisionAtaque = GetNode<CollisionShape3D>("AreaAtaqueCaida/Colision");
		cronometro = GetNode<Timer>("Cronometro");
		camara = GetNode<Node3D>("Camara");
		colision = GetNode<CollisionShape3D>("Colision");
		animacionCara = GetNode<AnimationPlayer>("Personaje/AnimacionCara");
		animacionEfectos = GetNode<AnimationPlayer>("Personaje/AnimacionEfectos");
		animacionModelo = GetNode<AnimationPlayer>("Personaje/Animacion");
	}

	private void SprintSwitch()
	{
		if (sprint)
		{
			velocidad = velocidadCorrer;
		}
		else
		{
			velocidad = velocidadBase;
		}

		if (Input.IsActionJustPressed("mayus") && !sprint)
		{
			sprint = true;
			return;
		}
		if (Input.IsActionJustPressed("mayus") && sprint)
		{
			sprint = false;
			return;
		}

	}
	private void CamaraPosition()
	{
		camara.Position = new Vector3(Position.X, Position.Y + 1, Position.Z);
	}	
	private void Direccion(double delta)
	{
		if (Velocity.X != 0 || Velocity.Z != 0)
		{		
		Rotation = jugador._Rotacion(delta,GirarPersonaje, Velocity, Rotation);
		}

		direccion.X = Input.GetAxis("a", "d");
		direccion.Z = Input.GetAxis("w", "s");
		direccion = direccion.Rotated(Vector3.Up, camara.Rotation.Y).Normalized();
	}
	private void EstaEnPisoOAire()
	{
		if (IsOnFloor())
		{	
			ResetJumpDash();
			if (IsOnFloor())
			{
				dobleSalto = false;
			}
		}
	}
	private void Efectos()
	{
		if (cronometro.TimeLeft != 0)
		{
			animacionEfectos.Play("Parpadeo");
			CollisionLayer = 2;
		}
		else
		{
			CollisionLayer = 1;
			animacionEfectos.Stop(); 
		}
	}
	private void ResetJumpDash()
	{
		sePuedeDesplazamiento = true;
		sePuedeDobleSalto = true;
	}
	private void CancelJumpDash()
	{
		dobleSalto = false;
		desplazamiento = false;
	}
	private async void GolpeControlador()
	{
		if (golpe)
		{
			await ToSignal(animacionModelo,"animation_finished");
			if (direccion.X != 0 || direccion.Z != 0 || Velocity.Y == 0)
			{
				golpe = false;
			}
		}
	}
	private void Sonido()
	{
		if (direccion != Vector3.Zero && IsOnFloor()&& !desplazamiento)
		{				
			if (velocidad == velocidadCorrer)
			{
				sfxPasos.StreamPaused = false;
				sfxPasos.PitchScale = 1.2f;
			}
			if (velocidad == velocidadBase)
			{
				sfxPasos.StreamPaused = false;
				sfxPasos.PitchScale = 0.8f;
			}
		}
		else
		{
			sfxPasos.StreamPaused = true;	
		}

		if (Salto())
		{
			sfx.salto.Play();
		}
	}
	private void ParticulasControlador()
	{
		if (direccion != Vector3.Zero && IsOnFloor()&& !desplazamiento)
		{				
			particulasCaminar.Emitting = true;
		}
		else
		{
			particulasCaminar.Emitting = false;
		}

	}

	public override void _Process(double delta)
	{
		Animaciones();
		
		CamaraPosition();
		Direccion(delta);
		SprintSwitch();
		EstaEnPisoOAire();
		Efectos();
		GolpeControlador();
		Sonido();
		ParticulasControlador();
	

		Gravedad(delta);
		Movimiento(delta);
		Salto();
		Dash(delta);
		Ataque();
	
		MoveAndSlide();
	}
	
	private async void Animaciones()
	{	
		if (atacando)
		{
			animacionModelo.Play("AtaqueCaida");
		}
		if (!atacando)
		{
			if (golpe)
			{
				animacionModelo.Play("Hit");
			}
			else
			{
				if (!desplazamiento)
				{
					if (!dobleSalto)
					{
						if (direccion != Vector3.Zero && IsOnFloor() && !sprint)
						{
							animacionModelo.Play("Walk");
	
						}
						else if (direccion != Vector3.Zero && IsOnFloor() && sprint)
						{
							animacionModelo.Play("Run");
					
						}
						else if (direccion == Vector3.Zero  && IsOnFloor())
						{
							animacionModelo.Play("Idle");
				
						}
					}
					if (!IsOnFloor() && !dobleSalto)
					{
						animacionModelo.Play("Fall");
					}
					if (!IsOnFloor() && dobleSalto)
					{
						animacionModelo.Play("Flip");
						await ToSignal(animacionModelo,"animation_finished");
						dobleSalto = false;
					}
				}
				else 
				{
					animacionModelo.Play("Dash");
				}

			}
		}
	}
	private void Gravedad(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity = jugador._Gravedad(delta,fuerzaGravedad,Velocity);
		}
	}
	private void Movimiento(double delta)
	{
		if (!golpe)
		{
			if (direccion != Vector3.Zero && !desplazamiento)
			{
				direccionDesplazamiento = direccion;
				if (IsOnFloor())
				{
					Velocity = jugador._CaminarCorrer(delta, velocidad,friccion,direccion, Velocity);
				}
				else
				{
					Velocity = jugador._CaminarCorrer(delta, velocidad,friccionAire,direccion, Velocity);
				}
			}
			else if (!desplazamiento)
			{
				if(IsOnFloor())
				{
					Velocity = jugador._Frenar(delta,friccion, Velocity);
				}
				else
				{
					Velocity = jugador._Frenar(delta,friccionAire, Velocity);
				}
			}
		}
		else
		{
			if (direccion == Vector3.Zero && !desplazamiento)
			{
				if(IsOnFloor())
				{
					Velocity = jugador._Frenar(delta,friccion * 0.1f, Velocity);
				}
				else
				{
					Velocity = jugador._Frenar(delta,friccionAire * 0.1f, Velocity);
				} 
			}
		}

	}
	private bool Salto()
	{
		Boolean salto = false;
		if (!atacando)
		{
			if (!golpe )
			{
				if (IsOnFloor())
				{
					if (Input.IsActionJustPressed("space"))
					{
						salto = true;
						JumpTween();
						if (desplazamiento)
						{
							desplazamiento = false;
						}
						Velocity = jugador._Salto(fuerzaSalto, Velocity);
					}
				}
			}
		}
		if (!IsOnFloor() && !golpe)
		{
			if (sePuedeDobleSalto && Input.IsActionJustPressed("space"))
			{
				atacando = false;
				salto = true;
				JumpTween();
				if (desplazamiento)
				{
					desplazamiento = false;
				}
				sePuedeDobleSalto = false;
				dobleSalto = true;
				Velocity = jugador._Salto(fuerzaSalto, Velocity);
			}
		} 		
		return salto;
	}
	private void Dash(double delta)
	{
		if (!atacando)
		{
			if (!golpe)
			{
				if (desplazamiento)
				{
					colision.Rotation = new Vector3(300, 0, 0);
					if (IsOnFloor())
					{
					Velocity = jugador._Frenar(delta,friccion * 0.15f, Velocity);
					}
					else
					{
					Velocity = jugador._Frenar(delta,friccionAire * 0.15f, Velocity);
					}
				}
				else
				{
					colision.Rotation = new Vector3(0,0,0);
				}
			}
		}
		if (Input.IsActionJustPressed("shift") && sePuedeDesplazamiento && !desplazamiento && !golpe)
		{
			atacando = false;
			sfx.Dash.Play();
			JumpTween();
			if (sprint)
			{
				velocidad = velocidadDesplazamiento * 1.25f;
			}
			else
			{
				velocidad = velocidadDesplazamiento;
			}
			sePuedeDobleSalto = true;
			desplazamiento = true;
			sePuedeDesplazamiento = false;
			Velocity = jugador._Dash(velocidad, direccionDesplazamiento, Velocity);
		}
	}
	private bool Ataque()
	{
		Boolean ataque = false;
		if (Input.IsActionJustPressed("control") && !IsOnFloor())
		{
			CancelJumpDash();
			Velocity = jugador._AtaqueCaida(-velocidadCaidaAtaque, Velocity);
			atacando = true;
		}

		if (atacando)
		{
			colisionAtaque.Disabled = false;
			if (IsOnFloor())
			{
				atacando = false;
				ataque = true;
			}
		}
		else
		{
			colisionAtaque.Disabled = true;
		}
		return ataque;
	
	}
	private void Daño(Vector3 empujeEnemigo, String tipoEnemigo, float fuerzaEmpuje)
	{
		CancelJumpDash();
		ResetJumpDash();
		Velocity = jugador._Golpe(empujeEnemigo, Velocity, fuerzaEmpuje);
		cronometro.Start();
		golpe = true;
		sfx.golpe.Play();

		vida -= 1;
		Salud();
	}

	private void Salud()
	{
		if (vida <= 0)
		{
			CallDeferred("CambiarEscena");
		}
	}
	private void CambiarEscena()
	{
    	GetTree().ChangeSceneToFile("res://ConstitucionEscena/GameOver.tscn");
	}

	private void AreaAtaqueCaida(Node3D body)
	{
		ResetJumpDash();
		atacando = false;
		particulasAtaque.Emitting = true;
		if (body.IsInGroup("Enemigo"))
		{
			body.Call("Daño",ataquCaida,dañoAtaqueCaida);
			Velocity = jugador._AtaqueCaida(velocidadCaidaAtaque / 2, Velocity);
		}
		if (body is StaticBody3D)
		{
			particulasAtaque.Emitting = true;
			ResetJumpDash();
			atacando = false;
			Velocity = jugador._AtaqueCaida(velocidadCaidaAtaque / 5, Velocity);
		}
	}

	private void JumpTween()
	{	
		Vector3 saltoEscala = new Vector3(0.8f,1.2f,0.8f);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "scale", saltoEscala, 0.1);
		tween.TweenProperty(this, "scale", new Vector3(1f,1f,1f), 0.1);
	}
}


