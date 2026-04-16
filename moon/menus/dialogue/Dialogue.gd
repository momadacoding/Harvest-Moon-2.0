extends Panel

func _ready():
	position = Vector2.ZERO
	size = get_viewport_rect().size

func _on_To_the_game_button_pressed():
	get_tree().change_scene_to_file("res://Game.tscn")
