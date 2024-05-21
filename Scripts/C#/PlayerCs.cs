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
	public float velocidad = 0;
	public float gravedad = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle() * 4;
	public Node3D rotador;
	public AnimationPlayer animacion;
	public Node3D modelo;
	public Vector3 saltoEscala = new Vector3(0.8f,1.2f,0.8f);
	public Vector3 desplazamientoDireccion = new Vector3(0,0,1);
	public Boolean sePuedeDobleSalto;
	public Boolean dobleSalto;
	public Boolean desplazamiento;
	public Boolean sePuedeDesplazamiento;
	public CpuParticles3D particulasCaminar;
	public AudioStreamPlayer3D sonidoPasos;
	public Boolean sprint;
	private AudioManager sfx;

    public override void _Ready()
    {	
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
	}

	public void _movimiento(double delta){
		Vector3 velocity = Velocity;
		if (desplazamiento)
		{

			velocity = new Vector3(desplazamientoDireccion.X * velocidad, velocity.Y ,desplazamientoDireccion.Z * velocidad);
			GD.Print(desplazamientoDireccion.Z * velocidad);
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

		if (!IsOnFloor())
			{
				velocity.Y -=  gravedad * (float)delta;
			}
			
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
				velocidad = velocidadDesplazamiento;
				sfx.salto.Play();
				sfx.salto.PitchScale = 1f;
				sePuedeDobleSalto = true;
				desplazamiento = true;
			}
			
			if (IsOnFloor())
			{
				sePuedeDesplazamiento = true;
				if(Input.IsActionJustPressed("space"))
				{
					sfx.salto.Play();
					sfx.salto.PitchScale = 0.7f;
					_JumpTween();
					velocity.Y = saltoFuerza;
					sePuedeDobleSalto = true;
				}
			}

			
			Vector3 direccion = Vector3.Zero;
			direccion.X = Input.GetAxis("a", "d");
			direccion.Z = Input.GetAxis("w", "s");
			direccion = direccion.Rotated(Vector3.Up, rotador.Rotation.Y).Normalized();
			if (direccion != Vector3.Zero)
			{
				desplazamientoDireccion = direccion;
			}
			velocity = new Vector3(direccion.X * velocidad, velocity.Y,direccion.Z * velocidad);
			
			if (IsOnFloor())
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

		Velocity = velocity;
		MoveAndSlide();
	}
	public void _camara(){
		rotador.Position = new Vector3(this.Position.X, this.Position.Y + 2, this.Position.Z);
	}
	public async void _animaciones(){
		
		if (desplazamiento)
		{
			particulasCaminar.Emitting = false;
				sonidoPasos.StreamPaused = true;
			animacion.Play("Dash");
			await ToSignal(animacion, "animation_finished");
			desplazamiento = false;
		}
		if (!desplazamiento)
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
				if (Velocity.X != 0 || Velocity.Z != 0)
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
	
}
