using Godot;
using System.Collections.Generic;

public partial class House : Node2D, IGameArea
{
    private Game _game = null!;
    private TileMap _objects = null!;

    private static readonly Vector2I GridSize = new(9, 9);

    public Vector2 tile_size { get; private set; }
    public Vector2 half_tile_size { get; private set; }
    public Vector2 grid_size { get; } = new(9, 9);

    private readonly List<List<int?>> _grid = new();

    public override void _Ready()
    {
        _game = GetParent<Game>();
        _objects = GetNode<TileMap>("Objects");

        tile_size = _game.tile_size;
        half_tile_size = _game.half_tile_size;

        for (int x = 0; x < GridSize.X; x++)
        {
            var col = new List<int?>();
            for (int y = 0; y < GridSize.Y; y++)
            {
                if (_objects.GetCellSourceId(0, new Vector2I(x, y)) != -1)
                    col.Add(1);
                else
                    col.Add(null);
            }
            _grid.Add(col);
        }
    }

    public bool teleport(Vector2 position)
    {
        return _objects.LocalToMap(position) == new Vector2I(4, 8);
    }

    public bool can_sleep(Vector2 position)
    {
        var cell = _objects.LocalToMap(position);
        return cell == new Vector2I(5, 3) || cell == new Vector2I(6, 4) || cell == new Vector2I(7, 4);
    }

    public bool is_cell_vacant(Vector2 pos, Vector2 direction)
    {
        var gridPos = _objects.LocalToMap(pos) + new Vector2I((int)direction.X, (int)direction.Y);

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
        var gridPos = _objects.LocalToMap(childNode.Position);
        _grid[gridPos.X][gridPos.Y] = null;

        var direction = childNode.Get("direction").AsVector2();
        var newGridPos = gridPos + new Vector2I((int)direction.X, (int)direction.Y);

        return _objects.MapToLocal(newGridPos) + half_tile_size;
    }
}
