using Godot;



public partial class Mapa : Node3D
{
	Jugador jugador; //Es necesario que el objeto sea tipo NombreScript
	[Export]
	public float gravedad =  39.2f; // public pq se comparte
	
	MapaPadre mapa;

	
	public override void _Ready()
	{
		mapa = new MapaPadre();
		jugador = GetNode<Jugador>("Jugador"); //Tomar el Nodo del jugador para otorgarle varialbes
	}

	public override void _Process(double delta)
	{
		ubicacionJugador(jugador);
	}

	private void ubicacionJugador(Jugador jugador)
	{
		GetTree().CallGroup("Enemigo", "ubicacionJugador",jugador.GlobalPosition);
	}

	private void AreaBody(Node3D body)
	{

		if(body.IsInGroup("Jugador"))
		{
			GetTree().ReloadCurrentScene();
		}
	}
	
}
