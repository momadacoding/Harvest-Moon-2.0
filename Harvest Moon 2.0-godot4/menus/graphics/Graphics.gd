extends Control

func _on_Resolution_Drop_Down_item_selected(ID):
	if ID == 0:
		get_window().set_size(Vector2(800,600))
	elif ID == 1:
		get_window().set_size(Vector2(1024,576))
	elif ID == 2:
		get_window().set_size(Vector2(1280,800))
	elif ID == 3:
		get_window().set_size(Vector2(1366,768))
	elif ID == 4:
		get_window().set_size(Vector2(1920,1080))
	elif ID == 5:
		get_window().set_size(Vector2(3840,2160))

func _on_Window_Drop_Down_item_selected(ID):
	if ID == 0:
		get_window().mode = Window.MODE_EXCLUSIVE_FULLSCREEN if (false) else Window.MODE_WINDOWED
		get_window().borderless = false
	elif ID == 1:
		get_window().mode = Window.MODE_EXCLUSIVE_FULLSCREEN if (true) else Window.MODE_WINDOWED
		get_window().borderless = true
	elif ID == 2:
		get_window().mode = Window.MODE_EXCLUSIVE_FULLSCREEN if (true) else Window.MODE_WINDOWED
		get_window().borderless = false