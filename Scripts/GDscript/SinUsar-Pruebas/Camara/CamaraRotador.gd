
extends Node3D
@export var sensibilidad = 1
@onready var camara = $Camera3D

var temporizadorRotacion : float = 7
var friccion : float = 0;
func _ready():
	pass


func _unhandled_input(event):
	
	if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		if event is InputEventMouseMotion:
			temporizadorRotacion = 7
			rotation_degrees.x -= event.relative.y * sensibilidad
			rotation_degrees.y -= event.relative.x * sensibilidad
			
		if event is InputEventMouseButton:
			if event.button_index == MOUSE_BUTTON_WHEEL_UP:
				camara.position = lerp(camara.position, Vector3(0,0,1), 0.2)
			if event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
				camara.position = lerp(camara.position, Vector3(0,0,1), -0.2)
	
func _process(delta):
	_mouse()
	camara.position.z = clamp(camara.position.z, 1,7)
	rotation_degrees.x = clamp(rotation_degrees.x, -90, 0)
#	temporizadorRotacion -= delta
#	if temporizadorRotacion <= 0:
#		temporizadorRotacion = 0
#		rotation_degrees.y += 1 * 0.2

func _mouse():

	if (Input.is_action_pressed("control")):
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
	else:
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

