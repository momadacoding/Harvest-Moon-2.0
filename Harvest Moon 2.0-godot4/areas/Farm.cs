using Godot;
using System.Collections.Generic;

public partial class Farm : Node2D, IGameArea
{
	private Game _game = null!;
	private GameSoundManager _soundManager = null!;
	private PlayerInventory _inventory = null!;

	private TileMap _crops = null!;
	private TileMap _dirt = null!;
	private TileMap _junk = null!;
	private TileMap _objects1 = null!;
	private TileMap _objects2 = null!;
	private TileMap _background2 = null!;

	private static readonly Vector2I[] TeleportAreas = { new(19, 8), new(19, -1) };
	private static readonly Vector2I GridSize = new(39, 23);

	public Vector2 tile_size { get; private set; }
	public Vector2 half_tile_size { get; private set; }
	public Vector2 grid_size { get; } = new(39, 23);

	private readonly List<List<int?>> _grid = new();

	private Vector2I TileCoords(Vector2 location) => new((int)location.X, (int)location.Y);
	private Vector2I GridDirection(Vector2 direction) => new((int)direction.X, (int)direction.Y);

	private int GetTile(TileMap tileMap, int x, int y) =>
		tileMap.GetCellSourceId(0, new Vector2I(x, y));

	private int GetTileV(TileMap tileMap, Vector2I location) =>
		tileMap.GetCellSourceId(0, location);

	private void SetTile(TileMap tileMap, int x, int y, int tileId) =>
		SetTileV(tileMap, new Vector2I(x, y), tileId);

	private void SetTileV(TileMap tileMap, Vector2I location, int tileId)
	{
		if (tileId == -1)
			tileMap.EraseCell(0, location);
		else
			tileMap.SetCell(0, location, tileId);
	}

	public override void _Ready()
	{
		_game = GetNode<Game>("/root/Game");
		_soundManager = GetNode<GameSoundManager>("/root/Game/Sound");

		_crops = GetNode<TileMap>("Crops");
		_dirt = GetNode<TileMap>("Dirt");
		_junk = GetNode<TileMap>("Junk");
		_objects1 = GetNode<TileMap>("Objects1");
		_objects2 = GetNode<TileMap>("Objects2");
		_background2 = GetNode<TileMap>("Background2");

		tile_size = _game.tile_size;
		half_tile_size = _game.half_tile_size;

		for (int x = 0; x < GridSize.X; x++)
		{
			var col = new List<int?>();
			for (int y = 0; y < GridSize.Y; y++)
			{
				if (GetTile(_objects1, x, y) != -1 || GetTile(_objects2, x, y) != -1)
					col.Add(1);
				else
					col.Add(null);
			}
			_grid.Add(col);
		}
	}

	private bool IsTeleportArea(Vector2I pos)
	{
		foreach (var area in TeleportAreas)
		{
			if (pos == area) return true;
		}
		return false;
	}

	public bool teleport(Vector2 position)
	{
		return IsTeleportArea(_objects1.LocalToMap(position));
	}

	public bool is_cell_vacant(Vector2 pos, Vector2 direction)
	{
		var gridPos = _objects1.LocalToMap(pos) + GridDirection(direction);

		if (IsTeleportArea(gridPos))
			return true;

		if (gridPos.X < GridSize.X && gridPos.X >= 0)
		{
			if (gridPos.Y < GridSize.Y && gridPos.Y >= 0)
			{
				if (_grid[gridPos.X][gridPos.Y] != 1)
					return true;
			}
		}

		return false;
	}

	public Vector2 update_child_pos(CharacterBody2D childNode)
	{
		var gridPos = _objects1.LocalToMap(childNode.Position);
		_grid[gridPos.X][gridPos.Y] = null;

		var newGridPos = gridPos + GridDirection(childNode.Get("direction").AsVector2());
		return _objects1.MapToLocal(newGridPos) + half_tile_size;
	}

	public void sleep()
	{
		for (int x = 0; x < GridSize.X; x++)
		{
			for (int y = 0; y < GridSize.Y; y++)
			{
				int cropTile = GetTile(_crops, x, y);
				int dirtTile = GetTile(_dirt, x, y);

				// grow turnips (0->1->2->3->4->5)
				if (cropTile >= 0 && cropTile <= 4 && dirtTile == 2)
				{
					SetTile(_crops, x, y, cropTile + 1);
					SetTile(_dirt, x, y, 0);
				}
				// grow strawberries (30->31->32->33->34->35)
				else if (cropTile >= 30 && cropTile <= 34 && dirtTile == 2)
				{
					SetTile(_crops, x, y, cropTile + 1);
					SetTile(_dirt, x, y, 0);
				}
				// grow eggplants (12->13->14->15->16->17)
				else if (cropTile >= 12 && cropTile <= 16 && dirtTile == 2)
				{
					SetTile(_crops, x, y, cropTile + 1);
					SetTile(_dirt, x, y, 0);
				}
				// unwatered tilled dirt without crops
				else if (dirtTile == 2)
				{
					SetTile(_dirt, x, y, 0);
				}

				// 5% chance to spawn junk on soil cells
				if (GetTile(_background2, x, y) == 15 && GetTile(_dirt, x, y) == -1 &&
					GetTile(_junk, x, y) == -1 && GD.Randi() % 20 + 1 == 1)
				{
					int junkType = (int)(GD.Randi() % 3 + 1);
					if (junkType == 1)
						SetTile(_junk, x, y, 4); // weeds
					else if (junkType == 2)
						SetTile(_junk, x, y, GD.Randi() % 2 + 1 == 1 ? 2 : 3); // rock
					else
						SetTile(_junk, x, y, 0); // wood
				}
			}
		}
	}

	private Vector2 OffsetPosition(Vector2 pos, string orientation)
	{
		if (orientation == "up") pos.Y -= tile_size.X;
		else if (orientation == "down") pos.Y += tile_size.X;
		else if (orientation == "right") pos.X += tile_size.X;
		else if (orientation == "left") pos.X -= tile_size.X;
		return pos;
	}

	public void smash_hammer(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);

		if (GetTileV(_crops, _crops.LocalToMap(pos)) == -1 &&
			(GetTileV(_dirt, _dirt.LocalToMap(pos)) == 0 || GetTileV(_dirt, _dirt.LocalToMap(pos)) == 2))
		{
			SetTileV(_dirt, _dirt.LocalToMap(pos), -1);
			if (!_soundManager.is_playing("hammer"))
			{
				_soundManager.play_tool("hammer");
				use_energy();
			}
		}
		else if (GetTileV(_junk, _junk.LocalToMap(pos)) == 2 || GetTileV(_junk, _junk.LocalToMap(pos)) == 3)
		{
			SetTileV(_junk, _junk.LocalToMap(pos), -1);
			if (!_soundManager.is_playing("hammer"))
			{
				_soundManager.play_tool("hammer");
				use_energy();
			}
		}
	}

	public void spread_seeds(Vector2 pos, int seedType)
	{
		if (_inventory == null)
			_inventory = GetNode<CharacterBody2D>("Player").GetNode<PlayerInventory>("UI/Inventory");

		int numSeeds = _inventory.get_amount(_inventory.equippedItem);
		float ts = tile_size.X;

		Vector2[] offsets = {
			new(ts, 0), new(-ts, 0), new(0, ts), new(0, -ts),
			new(ts, ts), new(ts, -ts), new(-ts, ts), new(-ts, -ts),
			Vector2.Zero
		};

		foreach (var offset in offsets)
		{
			var target = pos + offset;
			if (GetTileV(_dirt, _dirt.LocalToMap(target)) >= 0 &&
				GetTileV(_crops, _crops.LocalToMap(target)) == -1 &&
				numSeeds > 0)
			{
				SetTileV(_crops, _crops.LocalToMap(target), seedType);
				numSeeds--;
				_inventory.remove(_inventory.equippedItem);
				if (!_soundManager.is_playing("seeds"))
				{
					_soundManager.play_tool("seeds");
					use_energy();
				}
			}
		}
	}

	public void swing_hoe(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);

		float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
		if (orientation == "up" || orientation == "down")
		{ x1 = tile_size.X; x2 = -tile_size.X; }
		else
		{ y1 = tile_size.X; y2 = -tile_size.X; }

		Vector2[] targets = { pos, new(pos.X + x1, pos.Y + y1), new(pos.X + x2, pos.Y + y2) };

		foreach (var target in targets)
		{
			if (GetTileV(_background2, _background2.LocalToMap(target)) == 15 &&
				GetTileV(_dirt, _dirt.LocalToMap(target)) == -1 &&
				GetTileV(_junk, _junk.LocalToMap(target)) == -1)
			{
				SetTileV(_dirt, _dirt.LocalToMap(target), 0);
				if (!_soundManager.is_playing("hoe"))
				{
					_soundManager.play_tool("hoe");
					use_energy();
				}
			}
		}
	}

	public void swing_axe(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);

		int junkTile = GetTileV(_junk, _junk.LocalToMap(pos));
		if (junkTile == 0)
		{
			SetTileV(_junk, _junk.LocalToMap(pos), 1);
			if (!_soundManager.is_playing("axe"))
			{
				_soundManager.play_tool("axe");
				use_energy();
			}
		}
		else if (junkTile == 1 && !_soundManager.is_playing("axe"))
		{
			SetTileV(_junk, _junk.LocalToMap(pos), -1);
			if (!_soundManager.is_playing("axe"))
			{
				_soundManager.play_tool("axe");
				use_energy();
			}
		}
	}

	public void swing_sickle(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);

		if (GetTileV(_junk, _junk.LocalToMap(pos)) == 4)
		{
			SetTileV(_junk, _junk.LocalToMap(pos), -1);
			if (!_soundManager.is_playing("sickle"))
			{
				_soundManager.play_tool("sickle");
				use_energy();
			}
		}
	}

	public void swing_sickle_circle(Vector2 pos)
	{
		float ts = tile_size.X;
		Vector2[] offsets = {
			new(ts, 0), new(-ts, 0), new(0, ts), new(0, -ts),
			new(ts, ts), new(ts, -ts), new(-ts, ts), new(-ts, -ts),
			Vector2.Zero
		};

		foreach (var offset in offsets)
		{
			var target = pos + offset;
			if (GetTileV(_junk, _junk.LocalToMap(target)) == 4)
			{
				SetTileV(_junk, _junk.LocalToMap(target), -1);
				if (!_soundManager.is_playing("sickle"))
				{
					_soundManager.play_tool("sickle");
					use_energy();
				}
			}
		}
	}

	public void water_square(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);

		if (GetTileV(_dirt, _dirt.LocalToMap(pos)) == 0)
		{
			SetTileV(_dirt, _dirt.LocalToMap(pos), 2);
			if (!_soundManager.is_playing("watering"))
			{
				_soundManager.play_tool("watering");
				use_energy();
			}
		}
	}

	public int check_square_for_harvest(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);
		int cropTile = GetTileV(_crops, _crops.LocalToMap(pos));

		if (cropTile == 5) return 5;       // turnip
		if (cropTile == 35) return 35;     // strawberry
		if (cropTile == 17) return 17;     // eggplant

		return -1;
	}

	public void harvest_crop(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);
		SetTileV(_crops, _crops.LocalToMap(pos), -1);
		if (!_soundManager.is_playing("harvest"))
			_soundManager.play_effect("harvest");
	}

	public bool check_square_for_drop(Vector2 pos, string orientation)
	{
		pos = OffsetPosition(pos, orientation);

		if (GetTileV(_crops, _crops.LocalToMap(pos)) != -1) return false;
		if (GetTileV(_objects1, _objects1.LocalToMap(pos)) != -1) return false;
		if (GetTileV(_objects2, _objects2.LocalToMap(pos)) != -1) return false;
		if (IsTeleportArea(_objects2.LocalToMap(pos))) return false;

		return true;
	}

	public void drop_crop(Vector2 pos, string orientation, int cropNumber)
	{
		pos = OffsetPosition(pos, orientation);
		SetTileV(_crops, _crops.LocalToMap(pos), cropNumber);
		if (!_soundManager.is_playing("drop"))
			_soundManager.play_effect("drop");
	}

	public void simulate_rain()
	{
		for (int x = 0; x < GridSize.X; x++)
		{
			for (int y = 0; y < GridSize.Y; y++)
			{
				if (GetTile(_dirt, x, y) == 0)
					SetTile(_dirt, x, y, 2);
			}
		}
	}

	public void use_energy()
	{
		GetNode<EnergyBar>("Player/UI/Energy Bar").take_action();
	}
}
