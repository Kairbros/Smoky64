using Godot;


public partial class JugadorPadre 
{
	public Vector3 _CaminarCorrer(double delta, float velocidad, float friccion,Vector3 direccion, Vector3 Velocity)
	{
		Velocity = new Vector3(Mathf.Lerp(Velocity.X, direccion.X * velocidad, friccion * (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z, direccion.Z * velocidad, friccion * (float)delta));
		return Velocity;
	}
	public Vector3 _Frenar(double delta, float friccion, Vector3 Velocity)
	{   
		Velocity = new Vector3(Mathf.Lerp(Velocity.X, 0, friccion*  (float)delta), Velocity.Y, Mathf.Lerp(Velocity.Z,  0, friccion*  (float)delta));	
		return Velocity;
	}
    public Vector3 _Rotacion(double delta, float girarPersonaje, Vector3 Velocity, Vector3 rotacion)
	{
		Vector2 mirarDireccion = new Vector2(Velocity.Z, Velocity.X);
		rotacion = new Vector3(rotacion.X,Mathf.LerpAngle(rotacion.Y, mirarDireccion.Angle(), girarPersonaje * (float)delta),rotacion.Z);
		return rotacion;
	}
	public Vector3 _Salto(float saltoFuerza, Vector3 Velocity)
	{
		Velocity.Y = saltoFuerza;
		return Velocity;
	}
	public Vector3 _Dash(float velocidad, Vector3 direccionDesplazamiento, Vector3 Velocity)
	{
		Velocity = new Vector3(direccionDesplazamiento.X * velocidad, Velocity.Y ,direccionDesplazamiento.Z * velocidad);
		return Velocity;
	}
	public Vector3 _Gravedad(double delta, float gravedad, Vector3 Velocity)
	{
		Velocity.Y -= gravedad * (float)delta;
		return Velocity;
	}

	public Vector3 _Golpe(Vector3 empujeEnemigo, Vector3 Velocity, float fuerzaEmpuje)
	{
		Velocity = new Vector3(empujeEnemigo.X,fuerzaEmpuje,empujeEnemigo.Z);
		return Velocity;
	}
	public Vector3 _AtaqueCaida(float velCaidaAtaque, Vector3 Velocity)
	{
		Velocity.Y = velCaidaAtaque;
		return Velocity;
	}
}
