using Godot;

public partial class Player : CharacterBody2D
{
	public Node2D Zone = null!;

	private const int SleepDelay = 8000;
	public int sleepTime = -8000;

	private Game _game = null!;
	private GameSoundManager _soundManager = null!;
	private PlayerInventory Inventory = null!;
	private EnergyBar _energyBar = null!;
	private Control _shopMenu = null!;

	public int crop_number;
	public bool holdingItem;
	private Node2D _pickCrops = null!;
	private Sprite2D _eggplant = null!;
	private Sprite2D _strawberry = null!;
	private Sprite2D _turnip = null!;

	private const int HoldTimeDelay = 480;
	public int holdTime;
	public bool powerHold;
	public bool do_it_once;

	private static readonly Vector2 EggplantRight = new(7, -7);
	private static readonly Vector2 EggplantLeft = new(-7, -7);
	private static readonly Vector2 EggplantUp = new(1, -16);
	private static readonly Vector2 EggplantDown = new(1, -7);
	private static readonly Vector2 StrawberryRight = new(4, -7);
	private static readonly Vector2 StrawberryLeft = new(-5, -7);
	private static readonly Vector2 StrawberryUp = new(0, -15);
	private static readonly Vector2 StrawberryDown = new(0, -7);
	private static readonly Vector2 TurnipRight = new(4, -8);
	private static readonly Vector2 TurnipLeft = new(-5, -8);
	private static readonly Vector2 TurnipUp = new(0, -13);
	private static readonly Vector2 TurnipDown = new(0, -8);

	public string lastAnimation = "down";
	public string facingDirection = "down";
	public bool animationCommit;

	public bool teleport;

	public const int MAX_SPEED = 250;
	public float speed;
	public Vector2 direction;
	public Vector2 target_pos;
	public Vector2 target_direction;
	public bool is_moving;

	public bool stepLeft = true;
	public bool stepRight;
	private const int StepDelay = 250;
	public int stepTime = -250;

	private AnimatedSprite2D _sprite = null!;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("Sprite2D");
		_pickCrops = GetNode<Node2D>("PickCrops");
		_eggplant = GetNode<Sprite2D>("PickCrops/Eggplant");
		_strawberry = GetNode<Sprite2D>("PickCrops/Strawberry");
		_turnip = GetNode<Sprite2D>("PickCrops/Turnip");

		GetNode<DashboardTimeManager>("UI/Dashboard/TimeManager").sleep += _force_sleep;
	}

	public override void _EnterTree()
	{
		Zone = GetParent<Node2D>();
		_game = Zone.GetParent<Game>();
		_soundManager = GetNode<GameSoundManager>("/root/Game/Sound");
		Inventory = GetNode<PlayerInventory>("UI/Inventory");
		_energyBar = GetNode<EnergyBar>("UI/Energy Bar");
		_shopMenu = GetNode<Control>("/root/Game/Menus/Shop Menu");
		SetPhysicsProcess(true);
	}

	private IGameArea Area => (IGameArea)Zone;
	private Farm FarmZone => (Farm)Zone;

	private void _force_sleep()
	{
		if (Time.GetTicksMsec() > (ulong)(sleepTime + SleepDelay))
		{
			_sprite.Offset = Vector2.Zero;
			animationCommit = true;
			powerHold = false;
			lastAnimation = "pass out";
			sleepTime = (int)Time.GetTicksMsec();
			if (Zone.Name == "Farm")
				_soundManager.stop_music("farm");
			else
				_soundManager.stop_music("house");
			_soundManager.play_music("forceSleep");
			do_it_once = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_shopMenu.Visible) return;

		if (Area.teleport(Position))
			teleport = true;

		if (teleport && !is_moving)
		{
			teleport = false;
			if (Zone.Name == "Farm" && Position == new Vector2(624, -16))
				_game.farm_to_town();
			else if (Zone.Name == "Farm")
				_game.farm_to_house();
			else if (Zone.Name == "House")
				_game.house_to_farm();
			else if (Zone.Name == "Town")
				_game.town_to_farm();
		}

		// Tool actions
		if ((Input.IsActionPressed("ui_accept") && !holdingItem && !animationCommit &&
			 Zone.Name == "Farm" && _energyBar.has_energy()) || powerHold)
		{
			HandleToolActions();
		}

		// Crop holding logic
		if (holdingItem)
		{
			set_crop_offset();
			if (Input.IsActionPressed("B") && !animationCommit)
			{
				animationCommit = true;
				do_it_once = false;
				if (facingDirection == "left" || facingDirection == "right")
					lastAnimation = "store left";
				else if (facingDirection == "down")
					lastAnimation = "store down";
				else if (facingDirection == "up")
					lastAnimation = "store up";
			}
		}

		// Pickup/drop
		if (Input.IsActionPressed("Q") && !is_moving && Zone.Name == "Farm")
		{
			HandlePickupDrop();
		}

		// Sleep
		if (Input.IsActionPressed("E") && Zone.Name == "House" &&
			((House)Zone).can_sleep(Position) && !is_moving &&
			Time.GetTicksMsec() > (ulong)(sleepTime + SleepDelay))
		{
			Position = new Vector2(7 * 32, 2.25f * 32);
			animationCommit = true;
			lastAnimation = "pass out";
			sleepTime = (int)Time.GetTicksMsec();
			_soundManager.stop_music("house");
			_soundManager.play_music("forceSleep");
			do_it_once = false;
		}

		// Movement input
		direction = Vector2.Zero;
		if (!animationCommit)
		{
			if (Input.IsActionPressed("ui_up")) direction.Y = -1;
			else if (Input.IsActionPressed("ui_down")) direction.Y = 1;
			if (Input.IsActionPressed("ui_right") && !Input.IsActionPressed("shift_right_arrow")) direction.X = 1;
			else if (Input.IsActionPressed("ui_left") && !Input.IsActionPressed("shift_left_arrow")) direction.X = -1;
		}

		if (direction != Vector2.Zero)
		{
			speed = MAX_SPEED;
		}
		else
		{
			speed = 0;
			if (!animationCommit)
			{
				if (Input.IsActionPressed("W")) { lastAnimation = "up"; facingDirection = "up"; }
				else if (Input.IsActionPressed("S")) { lastAnimation = "down"; facingDirection = "down"; }
				else if (Input.IsActionPressed("D")) { _sprite.FlipH = true; lastAnimation = "right"; facingDirection = "right"; }
				else if (Input.IsActionPressed("A")) { _sprite.FlipH = false; lastAnimation = "left"; facingDirection = "left"; }
			}
		}

		// Start moving
		if (!is_moving && direction != Vector2.Zero && !Inventory.Visible)
		{
			target_direction = direction;
			if (Area.is_cell_vacant(Position, target_direction))
			{
				target_pos = Area.update_child_pos(this);
				is_moving = true;
				PlayWalkAnimation();
			}
		}
		else if (is_moving)
		{
			HandleMovement(delta);
		}
		else if (!is_moving)
		{
			PlayIdleOrActionAnimation();
		}

		TrackAnimations();
	}

	private void HandleToolActions()
	{
		if (Inventory.is_equipped("Hammer"))
		{
			animationCommit = true;
			lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "hammer left" :
							facingDirection == "down" ? "hammer down" : "hammer up";
		}
		else if (Inventory.is_equipped("StrawberrySeeds") || Inventory.is_equipped("TurnipSeeds") || Inventory.is_equipped("EggplantSeeds"))
		{
			animationCommit = true;
			lastAnimation = "seeds";
		}
		else if (Inventory.is_equipped("Hoe"))
		{
			animationCommit = true;
			lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "hoe left" :
							facingDirection == "down" ? "hoe down" : "hoe up";
		}
		else if (Inventory.is_equipped("Axe"))
		{
			animationCommit = true;
			lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "axe left" :
							facingDirection == "down" ? "axe down" : "axe up";
		}
		else if (Inventory.is_equipped("Sickle"))
		{
			HandleSickle();
		}
		else if (Inventory.is_equipped("Watering Can"))
		{
			HandleWateringCan();
		}
	}

	private void HandleSickle()
	{
		if (!powerHold)
		{
			powerHold = true;
			animationCommit = true;
			holdTime = (int)Time.GetTicksMsec();
			lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "sickle left" :
							facingDirection == "down" ? "sickle down" : "sickle up";
		}
		else if (!Input.IsActionPressed("ui_accept"))
		{
			powerHold = false;
			if (Time.GetTicksMsec() - (ulong)holdTime >= HoldTimeDelay)
				lastAnimation = "sickle circle";
		}
		else if (_sprite.Frame == 7)
		{
			_sprite.Frame = 6;
		}
	}

	private void HandleWateringCan()
	{
		if (!powerHold)
		{
			powerHold = true;
			animationCommit = true;
			holdTime = (int)Time.GetTicksMsec();
			lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "water left" :
							facingDirection == "down" ? "water down" : "water up";
		}
		else if (!Input.IsActionPressed("ui_accept"))
		{
			powerHold = false;
			if (Time.GetTicksMsec() - (ulong)holdTime >= HoldTimeDelay)
			{
				lastAnimation = facingDirection switch
				{
					"left" => "water circle left",
					"right" => "water circle right",
					"down" => "water circle down",
					_ => "water circle up"
				};
			}
		}
		else if (_sprite.Frame == 7)
		{
			_sprite.Frame = 6;
		}
	}

	private void HandlePickupDrop()
	{
		if (!holdingItem && !animationCommit)
		{
			crop_number = FarmZone.check_square_for_harvest(Position, facingDirection);
			if (crop_number != -1)
			{
				holdingItem = true;
				animationCommit = true;
				lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "pickup left" :
								facingDirection == "down" ? "pickup down" : "pickup up";
			}
		}
		else if (holdingItem && !animationCommit)
		{
			if (FarmZone.check_square_for_drop(Position, facingDirection))
			{
				holdingItem = false;
				animationCommit = true;
				lastAnimation = (facingDirection == "left" || facingDirection == "right") ? "drop left" :
								facingDirection == "down" ? "drop down" : "drop up";
			}
		}
	}

	private void PlayWalkAnimation()
	{
		if (direction.Y == -1 && (direction.X == 1 || direction.X == -1) && lastAnimation == "up")
		{
			_sprite.FlipH = false;
			_sprite.Play(holdingItem ? "Hold Walk Up" : "Walk Up");
			lastAnimation = "up"; facingDirection = "up";
		}
		else if (direction.Y == 1 && (direction.X == 1 || direction.X == -1) && lastAnimation == "down")
		{
			_sprite.FlipH = false;
			_sprite.Play(holdingItem ? "Hold Walk Down" : "Walk Down");
			lastAnimation = "down"; facingDirection = "down";
		}
		else
		{
			if (direction.X == 1)
			{
				_sprite.FlipH = true;
				_sprite.Play(holdingItem ? "Hold Walk Left" : "Walk Left");
				lastAnimation = "right"; facingDirection = "right";
			}
			else if (direction.X == -1)
			{
				_sprite.FlipH = false;
				_sprite.Play(holdingItem ? "Hold Walk Left" : "Walk Left");
				lastAnimation = "left"; facingDirection = "left";
			}
			else if (direction.Y == -1)
			{
				_sprite.FlipH = false;
				_sprite.Play(holdingItem ? "Hold Walk Up" : "Walk Up");
				lastAnimation = "up"; facingDirection = "up";
			}
			else if (direction.Y == 1)
			{
				_sprite.FlipH = false;
				_sprite.Play(holdingItem ? "Hold Walk Down" : "Walk Down");
				lastAnimation = "down"; facingDirection = "down";
			}
		}
	}

	private void HandleMovement(double delta)
	{
		if (stepLeft && !_soundManager.is_playing("rightFoot") && Time.GetTicksMsec() >= (ulong)(StepDelay + stepTime))
		{
			_soundManager.play_effect("leftFoot");
			stepRight = true; stepLeft = false;
			stepTime = (int)Time.GetTicksMsec();
		}
		else if (stepRight && !_soundManager.is_playing("leftFoot") && Time.GetTicksMsec() >= (ulong)(StepDelay + stepTime))
		{
			_soundManager.play_effect("rightFoot");
			stepLeft = true; stepRight = false;
			stepTime = (int)Time.GetTicksMsec();
		}

		speed = MAX_SPEED;
		Velocity = speed * target_direction * (float)delta;

		var distanceToTarget = new Vector2(
			Mathf.Abs(target_pos.X - Position.X),
			Mathf.Abs(target_pos.Y - Position.Y)
		);

		var vel = Velocity;
		if (Mathf.Abs(vel.X) > distanceToTarget.X)
		{
			vel.X = distanceToTarget.X * target_direction.X;
			is_moving = false;
		}
		if (Mathf.Abs(vel.Y) > distanceToTarget.Y)
		{
			vel.Y = distanceToTarget.Y * target_direction.Y;
			is_moving = false;
		}

		Velocity = vel;
		MoveAndCollide(Velocity);
	}

	private void PlayIdleOrActionAnimation()
	{
		string anim = lastAnimation switch
		{
			"up" => holdingItem ? "Hold Idle Up" : "Idle Up",
			"down" => holdingItem ? "Hold Idle Down" : "Idle Down",
			"left" or "right" => holdingItem ? "Hold Idle Left" : "Idle Left",
			"hammer up" => "Hammer Up",
			"hammer down" => "Hammer Down",
			"hammer left" => "Hammer Left",
			"seeds" => "Seeds",
			"hoe up" => "Hoe Up",
			"hoe down" => "Hoe Down",
			"hoe left" => "Hoe Left",
			"axe up" => "Axe Up",
			"axe down" => "Axe Down",
			"axe left" => "Axe Left",
			"sickle up" => "Sickle Up",
			"sickle down" => "Sickle Down",
			"sickle left" => "Sickle Left",
			"sickle circle" => "Sickle Circle",
			"water up" => "Water Up",
			"water down" => "Water Down",
			"water left" => "Water Left",
			"water circle up" => "Water Circle Up",
			"water circle down" => "Water Circle Down",
			"water circle left" => "Water Circle Left",
			"water circle right" => "Water Circle Right",
			"pickup up" => "Pickup Up",
			"pickup down" => "Pickup Down",
			"pickup left" => "Pickup Left",
			"drop up" => "Drop Up",
			"drop down" => "Drop Down",
			"drop left" => "Drop Left",
			"store up" => "Store Up",
			"store down" => "Store Down",
			"store left" => "Store Left",
			"pass out" => "Pass Out",
			_ => ""
		};

		if (!string.IsNullOrEmpty(anim))
			_sprite.Play(anim);
	}

	private void TrackAnimations()
	{
		// Hammer
		if (lastAnimation == "hammer left")
			play_animation(facingDirection == "right" ? 1 : -1, 0, 13, "hammer");
		else if (lastAnimation == "hammer up")
			play_animation(0, -1, 13, "hammer");
		else if (lastAnimation == "hammer down")
			play_animation(0, 1, 13, "hammer");
		// Seeds
		else if (lastAnimation == "seeds")
		{
			if (_sprite.Frame == 3)
			{
				if (Inventory.is_equipped("TurnipSeeds")) FarmZone.spread_seeds(Position, 0);
				else if (Inventory.is_equipped("StrawberrySeeds")) FarmZone.spread_seeds(Position, 30);
				else if (Inventory.is_equipped("EggplantSeeds")) FarmZone.spread_seeds(Position, 12);
				animationCommit = false;
				facingDirection = "down";
			}
		}
		// Hoe
		else if (lastAnimation == "hoe left")
			play_animation(facingDirection == "right" ? 1 : -1, 0, 12, "hoe");
		else if (lastAnimation == "hoe up")
			play_animation(0, -1, 12, "hoe");
		else if (lastAnimation == "hoe down")
			play_animation(0, 1, 12, "hoe");
		// Axe
		else if (lastAnimation == "axe left")
			play_animation(facingDirection == "right" ? 1 : -1, 0, 13, "axe");
		else if (lastAnimation == "axe up")
			play_animation(0, -1, 13, "axe");
		else if (lastAnimation == "axe down")
			play_animation(0, 1, 13, "axe");
		// Sickle
		else if (lastAnimation == "sickle left")
			play_animation(facingDirection == "right" ? 1 : -1, 0, 12, "sickle");
		else if (lastAnimation == "sickle up")
			play_animation(0, -1, 12, "sickle");
		else if (lastAnimation == "sickle down")
			play_animation(0, 1, 12, "sickle");
		// Sickle circle
		else if (lastAnimation == "sickle circle")
			TrackSickleCircle();
		// Water
		else if (lastAnimation == "water left")
			play_animation_water(facingDirection == "right" ? 1 : -1, 0);
		else if (lastAnimation == "water up")
			play_animation_water(0, -1);
		else if (lastAnimation == "water down")
			play_animation_water(0, 1);
		// Water circle
		else if (lastAnimation.StartsWith("water circle"))
			play_animation_water_circle();
		// Pickup
		else if (lastAnimation == "pickup left")
			play_animation_pickup(facingDirection == "right" ? 1 : -1, 0);
		else if (lastAnimation == "pickup up")
			play_animation_pickup(0, -1);
		else if (lastAnimation == "pickup down")
			play_animation_pickup(0, 1);
		// Drop
		else if (lastAnimation == "drop left")
			play_animation_drop(facingDirection == "right" ? 1 : -1, 0);
		else if (lastAnimation == "drop up")
			play_animation_drop(0, -1);
		else if (lastAnimation == "drop down")
			play_animation_drop(0, 1);

		// Store
		if (lastAnimation == "store left")
			play_animation_store(facingDirection == "right" ? 1 : -1, 0);
		else if (lastAnimation == "store up")
			play_animation_store(0, -1);
		else if (lastAnimation == "store down")
			play_animation_store(0, 1);

		// Pass out
		if (lastAnimation == "pass out")
		{
			if (_sprite.Frame == 15 && !do_it_once)
			{
				do_it_once = true;
				if (Zone.Name == "House")
					_game.house_to_sleep();
				else
					_game.teleport_player_to_bed();
				_game.GetNode<Farm>("Farm").sleep();
				_energyBar.reset_energy();
				_game.new_day();
				_soundManager.play_music("house");
				animationCommit = false;
				lastAnimation = "down";
			}
		}
	}

	private void reset_animation()
	{
		animationCommit = false;
		if (facingDirection == "left" || facingDirection == "right")
			lastAnimation = "left";
		else if (facingDirection == "up")
			lastAnimation = "up";
		else if (facingDirection == "down")
			lastAnimation = "down";
	}

	private void play_animation(int xMul, int yMul, int frameCount, string action)
	{
		int frame = _sprite.Frame;
		if (frame == frameCount - 2)
			_sprite.Offset = new Vector2(frame * xMul, frame * yMul);
		else if (frame == frameCount - 1)
			_sprite.Offset = new Vector2(frame / 2 * xMul, frame / 2 * yMul);
		else if (frame == frameCount)
		{
			_sprite.Offset = Vector2.Zero;
			reset_animation();
		}
		else
			_sprite.Offset = new Vector2(frame * 2 * xMul, frame * 2 * yMul);

		if (frame == 9)
		{
			if (action == "hammer") FarmZone.smash_hammer(Position, facingDirection);
			else if (action == "hoe") FarmZone.swing_hoe(Position, facingDirection);
			else if (action == "axe") FarmZone.swing_axe(Position, facingDirection);
			else if (action == "sickle") FarmZone.swing_sickle(Position, facingDirection);
		}
	}

	private void play_animation_water(int xMul, int yMul)
	{
		int frame = _sprite.Frame;
		if (frame == 22)
		{
			_sprite.Offset = Vector2.Zero;
			reset_animation();
		}
		else if (frame == 21)
			_sprite.Offset = new Vector2(10 * xMul, 10 * yMul);
		else if (frame == 17)
			FarmZone.water_square(Position, facingDirection);
		else if (frame <= 10)
			_sprite.Offset = new Vector2(frame * 2 * xMul, frame * 2 * yMul);
	}

	private void play_animation_water_circle()
	{
		_sprite.FlipH = false;
		int frame = _sprite.Frame;
		float ts = Area.tile_size.X;

		// Water circle frame-by-frame offset and water calls
		// Each direction has its own sequence of offsets and water_square calls
		if (facingDirection == "down") WaterCircleDown(frame, ts);
		else if (facingDirection == "up") WaterCircleUp(frame, ts);
		else if (facingDirection == "left") WaterCircleLeft(frame, ts);
		else if (facingDirection == "right") WaterCircleRight(frame, ts);
	}

	private void WaterCircleDown(int f, float ts)
	{
		if (f == 5) { _sprite.Offset = new(0, 5); FarmZone.water_square(Position, "down"); }
		else if (f == 6) _sprite.Offset = new(-5, 5);
		else if (f == 7) { _sprite.Offset = new(-10, 10); FarmZone.water_square(new(Position.X - ts, Position.Y), "down"); }
		else if (f == 8) _sprite.Offset = new(-5, 0);
		else if (f == 9) { _sprite.Offset = new(-10, 0); FarmZone.water_square(Position, "left"); }
		else if (f == 10) { _sprite.Offset = new(-5, -5); FarmZone.water_square(new(Position.X - ts, Position.Y), "up"); }
		else if (f == 11) _sprite.Offset = new(0, -5);
		else if (f == 12) { _sprite.Offset = new(0, -10); FarmZone.water_square(Position, "up"); }
		else if (f == 13) { _sprite.Offset = new(5, -5); FarmZone.water_square(new(Position.X, Position.Y - ts), "right"); }
		else if (f == 14) _sprite.Offset = new(5, 0);
		else if (f == 15) { _sprite.Offset = new(10, 0); FarmZone.water_square(Position, "right"); }
		else if (f == 16) _sprite.Offset = new(5, 5);
		else if (f == 17) { _sprite.Offset = new(10, 10); FarmZone.water_square(new(Position.X, Position.Y + ts), "right"); }
		else if (f == 18) { _sprite.Offset = new(0, 5); FarmZone.water_square(new(Position.X, Position.Y - ts), "down"); }
		else if (f == 19) _sprite.Offset = Vector2.Zero;
		else if (f == 20) animationCommit = false;
	}

	private void WaterCircleUp(int f, float ts)
	{
		if (f == 5) { _sprite.Offset = new(0, -5); FarmZone.water_square(Position, "up"); }
		else if (f == 6) _sprite.Offset = new(5, -5);
		else if (f == 7) { _sprite.Offset = new(10, -10); FarmZone.water_square(new(Position.X, Position.Y - ts), "right"); }
		else if (f == 8) _sprite.Offset = new(5, 0);
		else if (f == 9) { _sprite.Offset = new(10, 0); FarmZone.water_square(Position, "right"); }
		else if (f == 10) { _sprite.Offset = new(5, 5); FarmZone.water_square(new(Position.X, Position.Y + ts), "right"); }
		else if (f == 11) _sprite.Offset = new(0, 5);
		else if (f == 12) { _sprite.Offset = new(0, 10); FarmZone.water_square(Position, "down"); }
		else if (f == 13) { _sprite.Offset = new(-5, 5); FarmZone.water_square(new(Position.X - ts, Position.Y), "down"); }
		else if (f == 14) _sprite.Offset = new(-5, 0);
		else if (f == 15) { _sprite.Offset = new(-10, 0); FarmZone.water_square(Position, "left"); }
		else if (f == 16) _sprite.Offset = new(-5, -5);
		else if (f == 17) { _sprite.Offset = new(-10, -10); FarmZone.water_square(new(Position.X - ts, Position.Y), "up"); }
		else if (f == 18) { _sprite.Offset = new(0, -5); FarmZone.water_square(new(Position.X, Position.Y - ts), "down"); }
		else if (f == 19) _sprite.Offset = Vector2.Zero;
		else if (f == 20) animationCommit = false;
	}

	private void WaterCircleLeft(int f, float ts)
	{
		if (f == 5) { _sprite.Offset = new(-5, 0); FarmZone.water_square(Position, "left"); }
		else if (f == 6) _sprite.Offset = new(-5, -5);
		else if (f == 7) { _sprite.Offset = new(-10, -10); FarmZone.water_square(new(Position.X - ts, Position.Y), "up"); }
		else if (f == 8) _sprite.Offset = new(0, -5);
		else if (f == 9) { _sprite.Offset = new(0, -10); FarmZone.water_square(Position, "up"); }
		else if (f == 10) { _sprite.Offset = new(5, -5); FarmZone.water_square(new(Position.X, Position.Y - ts), "right"); }
		else if (f == 11) _sprite.Offset = new(5, 0);
		else if (f == 12) { _sprite.Offset = new(10, 0); FarmZone.water_square(Position, "right"); }
		else if (f == 13) { _sprite.Offset = new(5, 5); FarmZone.water_square(new(Position.X, Position.Y + ts), "right"); }
		else if (f == 14) _sprite.Offset = new(0, 5);
		else if (f == 15) { _sprite.Offset = new(0, 10); FarmZone.water_square(Position, "down"); }
		else if (f == 16) _sprite.Offset = new(-5, 5);
		else if (f == 17) { _sprite.Offset = new(-10, 10); FarmZone.water_square(new(Position.X - ts, Position.Y), "down"); }
		else if (f == 18) { _sprite.Offset = new(-5, 0); FarmZone.water_square(new(Position.X, Position.Y - ts), "down"); }
		else if (f == 19) _sprite.Offset = Vector2.Zero;
		else if (f == 20) animationCommit = false;
	}

	private void WaterCircleRight(int f, float ts)
	{
		if (f == 5) { _sprite.Offset = new(5, 0); FarmZone.water_square(Position, "right"); }
		else if (f == 6) _sprite.Offset = new(5, 5);
		else if (f == 7) { _sprite.Offset = new(10, 10); FarmZone.water_square(new(Position.X, Position.Y + ts), "right"); }
		else if (f == 8) _sprite.Offset = new(0, 5);
		else if (f == 9) { _sprite.Offset = new(0, 10); FarmZone.water_square(Position, "down"); }
		else if (f == 10) { _sprite.Offset = new(-5, 5); FarmZone.water_square(new(Position.X - ts, Position.Y), "down"); }
		else if (f == 11) _sprite.Offset = new(-5, 0);
		else if (f == 12) { _sprite.Offset = new(-10, 0); FarmZone.water_square(Position, "left"); }
		else if (f == 13) { _sprite.Offset = new(-5, -5); FarmZone.water_square(new(Position.X - ts, Position.Y), "up"); }
		else if (f == 14) _sprite.Offset = new(0, -5);
		else if (f == 15) { _sprite.Offset = new(0, -10); FarmZone.water_square(Position, "up"); }
		else if (f == 16) _sprite.Offset = new(5, -5);
		else if (f == 17) { _sprite.Offset = new(10, -10); FarmZone.water_square(new(Position.X, Position.Y - ts), "right"); }
		else if (f == 18) { _sprite.Offset = new(5, 0); FarmZone.water_square(new(Position.X, Position.Y - ts), "down"); }
		else if (f == 19) _sprite.Offset = Vector2.Zero;
		else if (f == 20) { _sprite.FlipH = true; animationCommit = false; }
	}

	private void TrackSickleCircle()
	{
		int f = _sprite.Frame;
		if (f == 1) { _sprite.Offset = Vector2.Zero; _sprite.Scale = new(1.075f, 1.075f); }
		else if (f == 2) _sprite.Scale = new(1.15f, 1.15f);
		else if (f == 3) { _sprite.Scale = new(1.075f, 1.075f); FarmZone.swing_sickle_circle(Position); }
		else if (f == 4) { _sprite.Scale = new(1, 1); animationCommit = false; facingDirection = "down"; }
	}

	private void play_animation_pickup(int xMul, int yMul)
	{
		int f = _sprite.Frame;
		Sprite2D crop = GetCropSprite();
		if (crop == null) return;

		Vector2 cropScale = crop_number == 17 ? new(.8f, .8f) : crop_number == 35 ? new(.9f, .9f) : new(1, 1);

		if (f == 0) _sprite.Offset = new(10 * xMul, 10 * yMul);
		else if (f == 1) _sprite.Offset = new(20 * xMul, 20 * yMul);
		else if (f == 2)
		{
			_sprite.Offset = new(10 * xMul, 10 * yMul);
			crop.Offset = new(20 * xMul, 20 * yMul);
			crop.Scale = cropScale;
			crop.Show();
			FarmZone.harvest_crop(Position, facingDirection);
		}
		else if (f == 3)
		{
			_sprite.Offset = Vector2.Zero;
			set_crop_offset();
			animationCommit = false;
		}
	}

	private void play_animation_drop(int xMul, int yMul)
	{
		int f = _sprite.Frame;
		Sprite2D crop = GetCropSprite();
		if (crop == null) return;

		if (f == 0) { _sprite.Offset = new(10 * xMul, 10 * yMul); crop.Offset = new(20 * xMul, 20 * yMul); }
		else if (f == 1)
		{
			_sprite.Offset = new(20 * xMul, 20 * yMul);
			crop.Hide();
			FarmZone.drop_crop(Position, facingDirection, crop_number);
		}
		else if (f == 2) _sprite.Offset = new(10 * xMul, 10 * yMul);
		else if (f == 3) { _sprite.Offset = Vector2.Zero; animationCommit = false; }
	}

	private void play_animation_store(int xMul, int yMul)
	{
		Sprite2D crop = GetCropSprite();
		if (crop == null) return;

		string cropName = crop_number == 17 ? "Eggplant" : crop_number == 35 ? "Strawberry" : "Turnip";
		Vector2 baseOffset = GetCropBaseOffset();
		int f = _sprite.Frame;

		if (facingDirection == "left" || facingDirection == "right")
		{
			float shrink1 = crop_number == 17 ? 0.6f : crop_number == 35 ? 0.7f : 0.8f;
			float shrink2 = crop_number == 17 ? 0.5f : crop_number == 35 ? 0.6f : 0.7f;

			if (f == 0) crop.Offset = new(xMul * -6 + baseOffset.X, -5 + baseOffset.Y);
			else if (f == 1) { crop.Offset = new(xMul * -11 + baseOffset.X, 0 + baseOffset.Y); crop.Scale = new(shrink1, shrink1); }
			else if (f == 2) { crop.Offset = new(xMul * -17 + baseOffset.X, 5 + baseOffset.Y); crop.Scale = new(shrink2, shrink2); }
			else if (f == 3) { crop.Hide(); FinishStore(cropName); }
		}
		else if (facingDirection == "up")
		{
			float shrink = crop_number == 17 ? 0.6f : crop_number == 35 ? 0.7f : 0.8f;
			if (f == 0) crop.Offset = new(baseOffset.X, yMul * 3 + baseOffset.Y);
			else if (f == 1) { crop.Offset = new(baseOffset.X, yMul * -5 + baseOffset.Y); _pickCrops.ShowBehindParent = false; }
			else if (f == 2) { crop.Offset = new(baseOffset.X, yMul * -15 + baseOffset.Y); crop.Scale = new(shrink, shrink); _pickCrops.ShowBehindParent = false; }
			else if (f == 3) { crop.Hide(); FinishStore(cropName); }
		}
		else if (facingDirection == "down")
		{
			if (f == 0) crop.Offset = new(baseOffset.X, yMul * -5 + baseOffset.Y);
			else if (f == 1) { crop.Offset = new(baseOffset.X, yMul * -10 + baseOffset.Y); _pickCrops.ShowBehindParent = true; }
			else if (f == 2) { crop.Offset = new(baseOffset.X, yMul * -7 + baseOffset.Y); _pickCrops.ShowBehindParent = true; }
			else if (f == 3) { crop.Hide(); FinishStore(cropName); }
		}
	}

	private void FinishStore(string cropName)
	{
		if (!do_it_once)
		{
			Inventory.add(cropName);
			do_it_once = true;
			_soundManager.play_effect("store");
		}
		animationCommit = false;
		holdingItem = false;
	}

	private Sprite2D GetCropSprite()
	{
		return crop_number switch
		{
			17 => _eggplant,
			35 => _strawberry,
			5 => _turnip,
			_ => null!
		};
	}

	private Vector2 GetCropBaseOffset()
	{
		if (crop_number == 17)
			return facingDirection == "left" ? EggplantLeft : facingDirection == "right" ? EggplantRight :
				   facingDirection == "up" ? EggplantUp : EggplantDown;
		if (crop_number == 35)
			return facingDirection == "left" ? StrawberryLeft : facingDirection == "right" ? StrawberryRight :
				   facingDirection == "up" ? StrawberryUp : StrawberryDown;
		return facingDirection == "left" ? TurnipLeft : facingDirection == "right" ? TurnipRight :
			   facingDirection == "up" ? TurnipUp : TurnipDown;
	}

	public void set_crop_offset()
	{
		_pickCrops.ShowBehindParent = facingDirection == "up";

		Sprite2D crop = GetCropSprite();
		if (crop != null)
			crop.Offset = GetCropBaseOffset();
	}
}
