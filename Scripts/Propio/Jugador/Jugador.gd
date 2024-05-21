extends CharacterBody3D

var velocidad : float = 0
@export var velocidadBase : float = 10
@export var velocidadCorrer : float = 15
@export var fuerzaSalto : float = 5
@export var SeguirCamara : float = 100
@export var CantidadSaltos : int = 2
@export var velocidadDesplazamiento : float = 30
var girarPersonajeVelocidad : int = 10
@export var SaltoEscala := Vector3(0.8, 1.2, 0.8)

@onready var modelo = $Personaje
@onready var animacion = $Personaje/Animacion
@onready var rotador = $Rotador
@onready var particulasPasos = $Personaje/ParticulasCaminar
@onready var pasosSonido = $Personaje/Pasos


var correr : bool = false
var aire : bool = false
var salto : bool = false
var dobleSalto : bool = false
var desplazamiento : bool = false

var desplazamientoDireccion = Vector3(1,0,0)
var gesto : bool = false
var gravedad = ProjectSettings.get_setting("physics/3d/default_gravity") * 3
# Called every frame. 'delta' is the elapsed time since the previous frame.

func _ready():
	velocidad = velocidadBase
func _physics_process(delta):
	var label = $Node2D/CanvasLayer/Label
	var fps = Engine.get_frames_per_second()

	label.text = str(fps)
	_reload(delta)
	_animaciones()

	rotador.position = Vector3(position.x, position.y + 2, position.z)
	
#	rotador.position = lerp(rotador.position, position, delta * SeguirCamara )
#	Interpole las 2 posiciones (posicion1 , posicion2, velocidad de interpolacion

	if _movimiento():
		var mirarDireccion = Vector2(velocity.z, velocity.x)
		modelo.rotation.y = lerp_angle(modelo.rotation.y,mirarDireccion.angle(),delta * girarPersonajeVelocidad)
		#interpole los 2 angulos (angle1, angle2, velocidad de rotacion)
		
	_moverse(delta)
	
func _moverse(delta):
	
		
	if _inputEvent():
		gesto = false
		
	if is_on_wall() and desplazamiento or _inputEvent() and desplazamiento:
		AudioManager.jump_sfx.play()
		AudioManager.jump_sfx.pitch_scale = 0.5
		desplazamiento = false
		if correr: 
			velocidad = velocidadCorrer
		if !correr:
			velocidad = velocidadBase
	if !is_on_floor():
		velocity.y -= gravedad * delta
	
		
	if !desplazamiento:
		var direccion = Vector3.ZERO
		direccion.x = Input.get_axis("a", "d")
		direccion.z = Input.get_axis("w", "s")
		direccion = direccion.rotated(Vector3(0,1,0), rotador.rotation.y).normalized()
		if direccion != Vector3.ZERO:
			desplazamientoDireccion = direccion
		velocity = Vector3(direccion.x * velocidad, velocity.y, direccion.z * velocidad)
	
	if Input.is_action_just_pressed("Shift") and !desplazamiento:
		_escala()
		AudioManager.jump_sfx.play()
		AudioManager.jump_sfx.pitch_scale = 2
		if correr:
			velocidad = velocidadDesplazamiento *1.5
		if !correr:
			velocidad = velocidadDesplazamiento
		salto = false
		dobleSalto = false
		velocity = Vector3(desplazamientoDireccion.x * velocidad, velocity.y, desplazamientoDireccion.z * velocidad)
		desplazamiento = true
		animacion.play("Dash")
		await animacion.animation_finished
		desplazamiento = false
		if correr:
			velocidad = velocidadCorrer
		if !correr:
			velocidad = velocidadBase

	if !desplazamiento:
		
	
		if correr:
			if Input.is_action_just_pressed("mayus"):
				velocidad = velocidadBase
				correr = false
				return
		if !correr:
			if Input.is_action_just_pressed("mayus"):
				velocidad = velocidadCorrer
				correr = true
				return
			
		
		if is_on_floor():
			if Input.is_action_just_pressed("ui_accept"):
				_escala()
				AudioManager.jump_sfx.play()
				AudioManager.jump_sfx.pitch_scale = 1
				salto = true
				dobleSalto = false
				velocity.y = fuerzaSalto 
		if !is_on_floor():
			if salto:
				if Input.is_action_just_pressed("ui_accept"):
					salto = false
					dobleSalto = true
					_escala()
					AudioManager.jump_sfx.play()
					AudioManager.jump_sfx.pitch_scale = 1.5
					velocity.y = fuerzaSalto 
					await animacion.animation_finished
					dobleSalto = false
		
	move_and_slide()
	
func _movimiento():
	return velocity.x or velocity.z
	
func _animaciones():
	if gesto:
		animacion.speed_scale = 2
	else:
		animacion.speed_scale = 1

	
	
	if !gesto:
		if !desplazamiento:
			if !is_on_floor() and dobleSalto:
				animacion.play("Flip")
			if !is_on_floor() and !dobleSalto:
				animacion.play("Fall")
				
			if is_on_floor():
				salto = false
				dobleSalto = false
				if _movimiento():
					pasosSonido.stream_paused = false
					particulasPasos.emitting = true
					
				if _movimiento() and !correr:
					pasosSonido.pitch_scale = 0.7
					particulasPasos.scale_amount_min = 1
					particulasPasos.scale_amount_max = 1.5
					animacion.play("Walk")
				if _movimiento() and correr:
					particulasPasos.scale_amount_min = 1.5
					particulasPasos.scale_amount_max = 2
					pasosSonido.pitch_scale = 1
					animacion.play("Run")
					
				if !_movimiento():
					pasosSonido.stream_paused = true
					particulasPasos.emitting = false
					animacion.play("Idle")
			else:
				pasosSonido.stream_paused = true
				particulasPasos.emitting = false
		else:
			pasosSonido.stream_paused = true
			particulasPasos.emitting = false

func _escala():

	var tween = get_tree().create_tween()
	tween.tween_property(self, "scale", SaltoEscala, 0.1)
	tween.tween_property(self, "scale", Vector3(1,1,1), 0.1)

func _inputEvent():
	return Input.is_action_just_pressed("ui_accept")

var cronometroReset : float = 0
func _reload(delta):
	if !is_on_floor():
		cronometroReset += delta
		
		if cronometroReset >= 5:
			get_tree().reload_current_scene()
	else:
		cronometroReset = 0
