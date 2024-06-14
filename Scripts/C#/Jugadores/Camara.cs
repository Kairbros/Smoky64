using Godot;
using System;


public partial class Camara : Node3D
{
	
	[Export] public float sensibilidad = 0.3f;
	public Vector3 rotacion;
	public Camera3D camara;
	public override void _Ready()
	{
		camara = GetNode<Camera3D>("Camara");
		Input.MouseMode = Input.MouseModeEnum.Captured; //Captura Mouse
	}

	public override void _Process(double delta)
	{

		camara.Position = new Vector3(0,0,Mathf.Clamp(camara.Position.Z,0,7));
		rotacion.X = Mathf.Clamp(rotacion.X,-90,30); //Clamp es un limitador que toma el valor minimo y el valor maximo
		RotationDegrees = rotacion; //Degress cambia los radianes a grados para un mejor control
	}

    public override void _UnhandledInput(InputEvent @event)
    {

        if (@event is InputEventMouseMotion inputEventMouse) //Evento de mouse
		{
			rotacion = RotationDegrees;

			rotacion.X -= inputEventMouse.Relative.Y  * sensibilidad;
			rotacion.Y -= inputEventMouse.Relative.X  * sensibilidad;
		
		}

		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				camara.Position = camara.Position.Lerp(new Vector3(0,0,1),0.2f);
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				camara.Position = camara.Position.Lerp(new Vector3(0,0,1),-0.2f);
			}
		}
    }
	
	
}
