using Godot;
using System.Collections.Generic;

public partial class Town : Node2D, IGameArea
{
    private Game _game = null!;
    private TileMap _dummyObject = null!;
    private TileMap _objects1 = null!;
    private TileMap _objects2 = null!;
    private TileMap _objects3 = null!;

    private static readonly Vector2I GridSize = new(75, 100);

    public Vector2 tile_size { get; private set; }
    public Vector2 half_tile_size { get; private set; }
    public Vector2 grid_size { get; } = new(75, 100);

    private readonly List<List<int?>> _grid = new();

    public override void _Ready()
    {
        _game = GetParent<Game>();
        _dummyObject = GetNode<TileMap>("DummyObject");
        _objects1 = GetNode<TileMap>("Objects1");
        _objects2 = GetNode<TileMap>("Objects2");
        _objects3 = GetNode<TileMap>("Objects3");

        tile_size = _game.tile_size;
        half_tile_size = _game.half_tile_size;

        for (int x = 0; x < GridSize.X; x++)
        {
            var col = new List<int?>();
            for (int y = 0; y < GridSize.Y; y++)
            {
                if (_objects1.GetCellSourceId(0, new Vector2I(x, y)) != -1 ||
                    _objects2.GetCellSourceId(0, new Vector2I(x, y)) != -1 ||
                    _objects3.GetCellSourceId(0, new Vector2I(x, y)) != -1)
                    col.Add(1);
                else
                    col.Add(null);
            }
            _grid.Add(col);
        }
    }

    public bool teleport(Vector2 position)
    {
        return _dummyObject.LocalToMap(position) == new Vector2I(37, 93);
    }

    public bool can_shop(Vector2 position)
    {
        return _dummyObject.LocalToMap(position) == new Vector2I(27, 43);
    }

    public bool is_cell_vacant(Vector2 pos, Vector2 direction)
    {
        var gridPos = _dummyObject.LocalToMap(pos) + new Vector2I((int)direction.X, (int)direction.Y);

        if (_grid[gridPos.X][gridPos.Y] != 1)
            return true;

        return false;
    }

    public Vector2 update_child_pos(CharacterBody2D childNode)
    {
        var gridPos = _dummyObject.LocalToMap(childNode.Position);
        _grid[gridPos.X][gridPos.Y] = null;

        var direction = childNode.Get("direction").AsVector2();
        var newGridPos = gridPos + new Vector2I((int)direction.X, (int)direction.Y);

        return _dummyObject.MapToLocal(newGridPos) + half_tile_size;
    }
}
