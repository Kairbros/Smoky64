using Godot;
using System;

public partial class PlayerCs : CharacterBody3D
{
	public float friccion;

	[Export] float girarPersonaje = 12;
	[Export] int velocidadBase = 7;
	[Export] int velocidadDesplazamiento = 25;
	[Export] int velocidadCorrer = 14;
	[Export] float saltoFuerza = 10;
	[Export] float velCaidaAtaque = 35;
	[Export] float velRebote = 10;
	[Export] float dañoAtaqueCaida = 1;
	float velocidad = 0;
	float friccionDash;
	float friccionAire = 10;
	float gravedad;
	Mapa mapa;
	Node3D rotador;
	Timer cronometro;
	AnimationPlayer animacion;
	Node3D modelo;
	Vector3 saltoEscala = new Vector3(0.8f,1.2f,0.8f);
	Vector3 desplazamientoDireccion = new Vector3(0,0,1);
	AudioStreamPlayer3D sonidoPasos;
	Boolean sePuedeDobleSalto;
	Boolean dobleSalto;
	Boolean desplazamiento;
	Boolean sePuedeDesplazamiento;
	Boolean golpe;
	Boolean sprint;
	Boolean invulnerable;
	Boolean ataque;
	AnimationPlayer cara;
	AnimationPlayer efectos;
	AudioManager sfx;
	CpuParticles3D particulasAtaque;
	CpuParticles3D particulasCaminar;
	CollisionShape3D colision;
	CollisionShape3D colisionAtaque;
	String ataquCaida = "AtaqueCaida";
	Vector3 direccion;
	public void tomarNodos()
	{
		sfx = GetNode<AudioManager>("/root/AudioManager"); // Tomar un objeto autoload
		particulasAtaque = GetNode<CpuParticles3D>("ParticulasAtaque");
		particulasCaminar = GetNode<CpuParticles3D>("ParticulasCaminar");

		cara = GetNode<AnimationPlayer>("Personaje/AnimacionCara");
		efectos = GetNode<AnimationPlayer>("Personaje/AnimacionEfectos");
		cronometro = GetNode<Timer>("Cronometro");
		colisionAtaque = GetNode<CollisionShape3D>("AreaAtaqueCaida/Colision");
		colision = GetNode<CollisionShape3D>("Colision");
		sonidoPasos = GetNode<AudioStreamPlayer3D>("Personaje/Pasos");
		rotador = GetNode<Node3D>("Rotador");
        modelo = GetNode<Node3D>("Personaje");
		animacion = GetNode<AnimationPlayer>("Personaje/Animacion");
	}
    public override void _Ready()
    {		
		tomarNodos();
		velocidad = velocidadBase;
		mapa = GetParent<Mapa>();  // Get_parent Variable
		gravedad = mapa.gravedad;

    }
    public override void _Process(double delta)
	{	
	
		if (Velocity.X != 0 || Velocity.Z != 0)
		{
			Vector2 mirarDireccion = new Vector2(Velocity.Z, Velocity.X);
			modelo.Rotation = new Vector3(modelo.Rotation.X,Mathf.LerpAngle(modelo.Rotation.Y, mirarDireccion.Angle(), girarPersonaje * (float)delta),modelo.Rotation.Z);
		
		} 

		if (desplazamiento)
		{
			colision.Rotation = new Vector3(300, modelo.Rotation.Y, 0);
		}
		else
		{
			colision.Rotation = new Vector3(0, modelo.Rotation.Y, 0);
		}
		_salto(delta);
		_dash(delta);
		_movimiento(delta);
		_ataque();
		_sprint();
		_camara();
		_animaciones();
		_colisiones();
		_estaEnPisoOAire();
		MoveAndSlide();
	
		double fps = Engine.GetFramesPerSecond();
		Label label = GetNode<Label>("FpsCount/HUD/Label");
		label.Text = fps.ToString();
	}
	public void _estaEnPisoOAire()
	{
		if (IsOnFloor())
		{	
			_resetJumpDash();
			
			if (Velocity.Y == 0)
			{
				dobleSalto = false;
			}
		}
	}
	public void _movimiento(double delta)
	{
	
		if (!IsOnFloor())
		{
			Vector3 velocity = Velocity;
			velocity.Y -= gravedad * (float)delta;
			Velocity = velocity;
		}

		if (!sprint)
		{
			velocidad = velocidadBase;
		}
		if (sprint)
		{
			velocidad = velocidadCorrer;
		}

		if (!desplazamiento)	
		{
			if (!golpe)
			{

				direccion.X = Input.GetAxis("a", "d");
				direccion.Z = Input.GetAxis("w", "s");
				direccion = direccion.Rotated(Vector3.Up, rotador.Rotation.Y).Normalized();
				if (direccion != Vector3.Zero)
				{
					desplazamientoDireccion = direccion;

					if (IsOnFloor())
					{
						Velocity = new Vector3(Mathf.Lerp(Velocity.X, direccion.X * velocidad, friccion * (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z, direccion.Z * velocidad, friccion * (float)delta));
					}
					else
					{
						Velocity = new Vector3(Mathf.Lerp(Velocity.X, direccion.X * velocidad, friccionAire * (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z, direccion.Z * velocidad, friccionAire * (float)delta));
					}
				}
				else
				{
					if (Velocity != Vector3.Zero)
					{	
						if (IsOnFloor())
						{
							Velocity = new Vector3(Mathf.Lerp(Velocity.X, 0, friccion*  (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z,  0, friccion*  (float)delta));	
						}
						else
						{
							Velocity = new Vector3(Mathf.Lerp(Velocity.X, 0, friccionAire*  (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z, 0, friccionAire*  (float)delta));	
						}
					}
				}	

			}
		}
			
	}
	public void _sprint()
	{		
		if (IsOnFloor() && !desplazamiento && !golpe)
		{
		
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
}
	public void _ataque()
	{

		if (!IsOnFloor())
		{
			if (Input.IsActionJustPressed("control") && !golpe) 
			{
				_cancelJumpDash();
				_resetJumpDash();
				ataque = true;
				Velocity = new Vector3(0,-velCaidaAtaque,0);
			}
		}	

		if (ataque)
		{	
			CollisionLayer = 3;
			colisionAtaque.Disabled = false;

			if(IsOnFloor())
			{
		
				Velocity = new Vector3(0,velRebote,0);
				particulasAtaque.Emitting = true;
				ataque = false;
	
				sfx.salto.Play();
				sfx.salto.PitchScale = 0.1f;
			}
		}
		else
		{
			CollisionLayer = 1;
			colisionAtaque.Disabled = true;
		}
	}
	public void _salto(double delta)
	{

		if (!ataque)
		{
		if (IsOnFloor() && !golpe)
				{
					
					if(Input.IsActionJustPressed("space") )
					{
						Vector3 velocity = Velocity;
						velocity.Y = saltoFuerza;
						Velocity = velocity;
						desplazamiento = false;
						sfx.salto.Play();
						sePuedeDobleSalto = true;
						sfx.salto.PitchScale = 0.7f;
						_JumpTween();
					}
				}
				if (!IsOnFloor())
				{
					if (sePuedeDobleSalto)
					{
						if(Input.IsActionJustPressed("space"))
						{	
							Vector3 velocity = Velocity;
							velocity.Y = saltoFuerza;
							Velocity = velocity;
							desplazamiento = false;
							sfx.salto.Play();
							sfx.salto.PitchScale = 1f;
							_JumpTween();
							sePuedeDobleSalto = false;
							dobleSalto = true;
						}
					}
				}
		}

	}
	public void _dash(double delta)
	{	

		if (!ataque)
		{
			if(desplazamiento)
			{
				friccionDash =  friccion * 0.15f;
				if(IsOnFloor())
				{
					Velocity = new Vector3(Mathf.Lerp(Velocity.X, 0, friccionDash* (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z, 0, friccionDash* (float)delta));	
				}
				else
				{
					Velocity = new Vector3(Mathf.Lerp(Velocity.X, 0, friccionDash*  (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z, 0, friccionDash*  (float)delta));	
				}
			}	
		

		if (Input.IsActionJustPressed("shift") && sePuedeDesplazamiento && !desplazamiento && !golpe)
			{
				sePuedeDesplazamiento = false;

				if (!sprint)
				{
					velocidad = velocidadDesplazamiento;
				}
				else
				{
					velocidad = velocidadDesplazamiento * 1.5f;
				}
				Velocity = new Vector3(desplazamientoDireccion.X * velocidad, Velocity.Y ,desplazamientoDireccion.Z * velocidad);
				sfx.salto.Play();
				sfx.salto.PitchScale = 1f;
				sePuedeDobleSalto = true;
				desplazamiento = true;
			}
		}
	}
	public void _camara(){
		rotador.Position = new Vector3(this.Position.X, this.Position.Y + 1, this.Position.Z);
	}
	public async void _animaciones()
	{
		if (ataque)
		{
			animacion.Play("AtaqueCaida");
		}

		if (!ataque)
		{
			if (desplazamiento || golpe)
			{
				particulasCaminar.Emitting = false;
				sonidoPasos.StreamPaused = true;
			}

			if (desplazamiento)
			{
				animacion.Play("Dash");
				await ToSignal(animacion, "animation_finished"); //Basicamente lo mismo que await animation finished solo que esta se escribe asi y convierte la funcion en "Async"
				if (Input.IsActionJustPressed("space"))
				{
					desplazamiento = false;
				}
				
			}

			if (golpe)
			{
				invulnerable = true;
				animacion.Play("Hit");
				await ToSignal(animacion, "animation_finished");
				golpe = false;
				

			}	
			if (!desplazamiento || !golpe)
			{
				if (!IsOnFloor() && !dobleSalto)
				{
					if (!desplazamiento)
					{
					animacion.Play("Fall");
					}
				}
				if (dobleSalto)
				{
					animacion.Play("Flip");
					await ToSignal(animacion, "animation_finished");
					dobleSalto = false;
					
				}
			

				if (IsOnFloor())
				{
					if (direccion != Vector3.Zero)
					{
						particulasCaminar.Emitting = true;
						sonidoPasos.StreamPaused = false;

						if (velocidad == velocidadBase)
						{
							animacion.Play("Walk");
						}
						if (velocidad == velocidadCorrer)
						{
							animacion.Play("Run");
						}
					}
					else
					{
						animacion.Play("Idle");
						particulasCaminar.Emitting = false;
						sonidoPasos.StreamPaused = true;
					}
				}
				else
				{
					particulasCaminar.Emitting = false;
					sonidoPasos.StreamPaused = true;
				}
			}
		}
	
	}
	public void _JumpTween()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "scale", saltoEscala, 0.1);
		tween.TweenProperty(this, "scale", new Vector3(1f,1f,1f), 0.1);
	}
	public void _colisiones()

	{
		if (invulnerable)
		{
			efectos.Play("Parpadeo");
		}
		else
		{
			efectos.Stop(); 
		}
	}
	public void _resetJumpDash()
	{
		sePuedeDesplazamiento = true;
		sePuedeDobleSalto = true;
	}
	public void _cancelJumpDash()
	{
		dobleSalto = false;
		desplazamiento = false;
	}
	public void Cronometro()
	{
		invulnerable = false;
	}
	public void Daño(String tipoEnemigo, Vector3 empujeEnemigo) //Es una funcion que recibe parametros y se inicializa desde otro objeto bajo cierta condicion
	{
		golpe = true;
		_cancelJumpDash();
		_resetJumpDash();
		cronometro.Start();
		_JumpTween();
		sfx.salto.Play();
		sfx.salto.PitchScale = 0.4f ;
		Velocity = Vector3.Zero; 
		Velocity = new Vector3(empujeEnemigo.X , empujeEnemigo.Y, empujeEnemigo.Z);
	}
	public void ataqueCaida(Node3D body) // Es una señal enviada por el nodo, esta se debe llamar igual y recibir los mismos parametros
		{
	
			if (body.IsInGroup("Enemigo"))
			{
				sfx.salto.Play();
				sfx.salto.PitchScale = 0.1f;
				particulasAtaque.Emitting = true;
				body.Call("Daño",ataquCaida,dañoAtaqueCaida); // le llamo al cuerpo que entre una funcion, esta debe llamarse igual y pasar los mismos parametros que recive
				ataque = false;
				Velocity =  new Vector3(Velocity.X, velCaidaAtaque - 10, Velocity.Y) ;
			}
		

		}
}
