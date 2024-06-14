
using System;
using Godot;



public partial class EnemigoPadre : Node
{
	public Vector3 _movimiento(Vector3 Velocity, Vector3 GlobalPosition,Vector3 SiguienteDireccion, float Velocidad)
	{
		Vector3 Direccion;
		Direccion = SiguienteDireccion - GlobalPosition;
		Direccion = Direccion.Normalized();
		Velocity = Velocity.Lerp(Direccion*Velocidad, 1);
		return Velocity;
	}
	public Vector3 _gravedad(double delta, float gravedad, Vector3 Velocity)
	{
		Velocity.Y -= gravedad * (float)delta;
		return Velocity;
	}
	public bool _colisiones(Timer cronometro)
	{
		bool desactivada = false;
		if (cronometro.TimeLeft > 0.0)
		{
			desactivada = true;
		}
		if (cronometro.TimeLeft == 0.0)
		{
			desactivada = false;
		}
		return desactivada;
	}
	public Vector3 _empujeEnemigo(Vector3 bodyGlobalPosition, Vector3 globalPosition, float velocidad)
	{
		Vector3 direccionEmpuje;
		direccionEmpuje = (bodyGlobalPosition - globalPosition).Normalized();
		direccionEmpuje = direccionEmpuje * velocidad;
		return direccionEmpuje;
	}
	
	public void _animaciones(AnimationPlayer animacion, Vector3 Velocity, AnimationPlayer animacionEfectos, Boolean golpeado)
	{
			if (Velocity != Vector3.Zero)
			{
				animacion.Play("Caminar");
			}
			else
			{
				animacion.Play("Idle");
			}

			if (golpeado)
			{
				animacionEfectos.Play("Parpadeo");
			}
			else
			{
				animacionEfectos.Stop();
			}
	}
	public bool _muerte(float vida)
	{
		bool estaMuerto = false;
		if (vida <= 0)
		{	
			estaMuerto= true;
		}
		return estaMuerto;
	}

}
