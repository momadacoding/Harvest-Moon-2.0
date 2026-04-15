extends Node2D

@onready var Game = get_node("/root/Game")

var tweenerDuration
var _active_tweens = {}

const viewport_size = Vector2(480, 270) #based on camera, 1366x768 * .35 zoom

#for the first day of the game, morning has to be manually called to fade out
func _ready():
	set_process(true)
	fade_in_shader("afternoon")

#position the shaders directly over the player
func _process(delta):
	position = Vector2(Game.player.position.x - viewport_size.x / 2, Game.player.position.y - viewport_size.y / 2) + Game.player_location.position

#resets the tweeners for a new day and begins fading in the afternoon
func new_day():
	_reset_tweeners()
	fade_in_shader("afternoon")

#fades in tweeners based upon the time of day
func fade_in_shader(time):
	if tweenerDuration == null:
		#shaders fade in and out in sets of 5 hours
		tweenerDuration = get_node("/root/Game/Farm/Player/UI/Dashboard/TimeManager/Time/Timer").wait_time * 300

	if time == "afternoon":
		_tween_shader("TweenMorningOut", $Morning, Color(0.79, 0.79, 0.32, 0.35), Color(0.79, 0.79, 0.32, 0.0), Tween.EASE_OUT)
		_tween_shader("TweenAfternoonIn", $Afternoon, Color(1, 1, 1, 0), Color(1, 1, 1, 0), Tween.EASE_OUT)
	elif time == "evening":
		_tween_shader("TweenAfternoonOut", $Afternoon, Color(1, 1, 1, 0), Color(1, 1, 1, 0), Tween.EASE_IN)
		_tween_shader("TweenEveningIn", $Evening, Color(1, 0.33, 0, 0), Color(1, 0.33, 0, 0.25), Tween.EASE_IN)
	elif time == "night":
		_tween_shader("TweenEveningOut", $Evening, Color(1, 0.33, 0, 0.25), Color(1, 0.33, 0, 0), Tween.EASE_IN)
		_tween_shader("TweenNightIn", $Night, Color(0.05, 0.09, 0.15, 0), Color(0.05, 0.09, 0.15, 0.75), Tween.EASE_IN)

func restore_state(morning_alpha, afternoon_alpha, evening_alpha, night_alpha):
	_reset_tweeners()
	$Morning.color = Color(0.79, 0.79, 0.32, morning_alpha)
	$Afternoon.color = Color(1, 1, 1, afternoon_alpha)
	$Evening.color = Color(1, 0.33, 0, evening_alpha)
	$Night.color = Color(0.05, 0.09, 0.15, night_alpha)

func get_tween_progresses():
	return {
		"TweenMorningOut": 0.0,
		"TweenAfternoonIn": 0.0,
		"TweenAfternoonOut": 0.0,
		"TweenEveningIn": 0.0,
		"TweenEveningOut": 0.0,
		"TweenNightIn": 0.0
	}

#resets the tweeners for a new day
func _reset_tweeners():
	for tween_name in _active_tweens.keys():
		var tween = _active_tweens[tween_name]
		if is_instance_valid(tween):
			tween.kill()
	_active_tweens.clear()

	#reset any shaders on the screen, setting the morning to full brightness
	$Morning.color = Color(0.79, 0.79, 0.32, 0.35)
	$Afternoon.color = Color(1, 1, 1, 0)
	$Evening.color = Color(1, 0.33, 0, 0)
	$Night.color = Color(0.05, 0.09, 0.15, 0)

func _tween_shader(tween_name, canvas_item, from_color, to_color, ease):
	if _active_tweens.has(tween_name):
		var existing_tween = _active_tweens[tween_name]
		if is_instance_valid(existing_tween):
			existing_tween.kill()

	canvas_item.color = from_color
	var tween = create_tween()
	tween.set_trans(Tween.TRANS_LINEAR)
	tween.set_ease(ease)
	tween.tween_property(canvas_item, "color", to_color, tweenerDuration)
	_active_tweens[tween_name] = tween

#show shaders if they were hidden or hides them if they were shown (for indoor vs. outdoor settings)
func toggle_shaders():
	visible = not visible
