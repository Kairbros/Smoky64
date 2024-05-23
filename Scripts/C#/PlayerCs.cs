using Godot;
using System;
public partial class PlayerCs : CharacterBody3D
{
	[Export]
	public float girarPersonaje = 12;
	[Export]
	public int velocidadBase = 7;
	[Export]
	public int velocidadDesplazamiento = 30;
	[Export]
	public int velocidadCorrer = 14;
	[Export]
	public float saltoFuerza = 10;
	[Export]
	public float friccion = 0.01f;
	public float velocidad = 0;
	public float gravedad;
	public Mapa mapa;
	public Node3D rotador;
	public Timer cronometro;
	public AnimationPlayer animacion;
	public Node3D modelo;
	public Vector3 saltoEscala = new Vector3(0.8f,1.2f,0.8f);
	public Vector3 desplazamientoDireccion = new Vector3(0,0,1);
	public CpuParticles3D particulasCaminar;
	public AudioStreamPlayer3D sonidoPasos;

	public Boolean sePuedeDobleSalto;
	public Boolean dobleSalto;
	public Boolean desplazamiento;
	public Boolean sePuedeDesplazamiento;
	public Boolean golpe;
	public Boolean sprint;
	public Boolean invulnerable;
	public AnimationPlayer cara;
	public AnimationPlayer efectos;

	private AudioManager sfx;
	public CollisionShape3D colision;

	public Vector3 direccion;
    public override void _Ready()
    {	
		mapa = GetParent<Mapa>();
		cara = GetNode<AnimationPlayer>("Personaje/AnimacionCara");
		efectos = GetNode<AnimationPlayer>("Personaje/AnimacionEfectos");
		cronometro = GetNode<Timer>("Cronometro");
		colision = GetNode<CollisionShape3D>("Colision");
		gravedad = mapa.gravedad;
		sfx = GetNode<AudioManager>("/root/AudioManager");
		velocidad = velocidadBase;
		sonidoPasos = GetNode<AudioStreamPlayer3D>("Personaje/Pasos");
		particulasCaminar = GetNode<CpuParticles3D>("Personaje/ParticulasCaminar");
		rotador = GetNode<Node3D>("Rotador");
        modelo = GetNode<Node3D>("Personaje");
		animacion = GetNode<AnimationPlayer>("Personaje/Animacion");
    }
    public override void _Process(double delta)
	{

		if (desplazamiento)
		{
		colision.Rotation = new Vector3(modelo.Rotation.X  + 300,  modelo.Rotation.Y,modelo.Rotation.Z);
		}
		else
		{
		colision.Rotation = new Vector3(modelo.Rotation.X, modelo.Rotation.Y,modelo.Rotation.Z);
		}
		_movimiento(delta);
		double fps = Engine.GetFramesPerSecond();

		Label label = GetNode<Label>("Node2D/CanvasLayer/Label");

		label.Text = fps.ToString();
		if (Velocity.X != 0 || Velocity.Z != 0){
			Vector2 mirarDireccion = new Vector2(Velocity.Z, Velocity.X);
			Vector3 rotacion = modelo.Rotation;
			rotacion.Y = Mathf.LerpAngle(rotacion.Y, mirarDireccion.Angle(), girarPersonaje * (float)delta);
			modelo.Rotation = rotacion;
			
		} 
		_camara();
		_animaciones();
		_colisiones();
	}

	public void _movimiento(double delta){
	
		Vector3 velocity = Velocity;
		

		if (desplazamiento)
		{
			velocity = new Vector3(desplazamientoDireccion.X * velocidad, velocity.Y ,desplazamientoDireccion.Z * velocidad);
		}

		if (!golpe)
		{
		if (IsOnFloor())
				{
					sePuedeDesplazamiento = true;
					if(Input.IsActionJustPressed("space"))
					{
						desplazamiento = false;
						sfx.salto.Play();
						sfx.salto.PitchScale = 0.7f;
						_JumpTween();
						velocity.Y = saltoFuerza;
						sePuedeDobleSalto = true;
					}
				}
		if (!IsOnFloor())
			{
			if (sePuedeDobleSalto)
			{
				if(Input.IsActionJustPressed("space"))
				{	
					desplazamiento = false;
					sfx.salto.Play();
					sfx.salto.PitchScale = 1f;
					_JumpTween();
					velocity.Y = saltoFuerza;
					sePuedeDobleSalto = false;
					dobleSalto = true;
				}
			}
			}
		}

			if (IsOnWall())
			{
				desplazamiento = false;

				if (sprint)
				{
					velocidad = velocidadCorrer;
				}
				else
				{
					velocidad = velocidadBase;
				}
			}

		if (!IsOnFloor())
			{
				velocity.Y -=  gravedad * (float)delta;
			}
		
		if (!golpe)
		{
	
			if (!desplazamiento)
			{
				if (!sprint)
				{
				velocidad = velocidadBase;
				}
				if (sprint)
				{
				velocidad = velocidadCorrer;
				}

				if (Input.IsActionJustPressed("shift") && sePuedeDesplazamiento)
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
					sfx.salto.Play();
					sfx.salto.PitchScale = 1f;
					sePuedeDobleSalto = true;
					desplazamiento = true;
				}
				
				direccion.X = Input.GetAxis("a", "d");
				direccion.Z = Input.GetAxis("w", "s");
				direccion = direccion.Rotated(Vector3.Up, rotador.Rotation.Y).Normalized();
				if (direccion != Vector3.Zero)
				{
					desplazamientoDireccion = direccion;

					velocity.X = Mathf.Lerp(velocity.X, velocidad *direccion.X, friccion);
					velocity.Z = Mathf.Lerp(velocity.Z, velocidad *direccion.Z, friccion);
				}
				else
				{
					if (velocity != Vector3.Zero)
					{
						velocity.X -= velocity.X * friccion; 
						velocity.Z -= velocity.Z * friccion; 
					}
				}
			
				
				if (IsOnFloor())
				{
					dobleSalto = false;
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
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	public void _camara(){
		rotador.Position = new Vector3(this.Position.X, this.Position.Y + 1, this.Position.Z);
	}
	public async void _animaciones(){
		
		if (desplazamiento || golpe)
		{
			particulasCaminar.Emitting = false;
			sonidoPasos.StreamPaused = true;
		}

		if (desplazamiento)
		{
			animacion.Play("Dash");
			await ToSignal(animacion, "animation_finished");
			desplazamiento = false;
		}

		if (golpe)
		{
			invulnerable = true;
			efectos.Play("Parpadeo");
			animacion.Play("Hit");
			await ToSignal(animacion, "animation_finished");
			golpe = false;
			

		}
		if (!desplazamiento || !golpe)
		{
			if (!IsOnFloor() && !dobleSalto)
			{
				animacion.Play("Fall");
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
	public void _JumpTween(){
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "scale", saltoEscala, 0.1);
		tween.TweenProperty(this, "scale", new Vector3(1f,1f,1f), 0.1);
	}
	public void _BlinkTween(){
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "modulate", saltoEscala, 0.1);
		tween.TweenProperty(this, "scale", new Vector3(1f,1f,1f), 0.1);
	}
	
	public void Daño(String tipoEnemigo, Vector3 empujeEnemigo)
	{
		cronometro.Start();
		golpe = true;
		Velocity = Vector3.Zero; 
		_JumpTween();
		sfx.salto.Play();
		sfx.salto.PitchScale = 0.4f ;
		Velocity = new Vector3(empujeEnemigo.X * 1 , empujeEnemigo.Y, empujeEnemigo.Z* 1);
		
	}

	public void Cronometro()
	{
		invulnerable = false;
	}

	public void _colisiones()

	{
		if (invulnerable)
		{
			CollisionLayer = 2;
		}
		else
		{
			efectos.Stop(); 
			CollisionLayer = 1;
		}
	}

}
