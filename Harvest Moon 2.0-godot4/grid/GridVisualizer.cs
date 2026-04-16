using Godot;

public partial class GridVisualizer : Node2D
{
    public override void _Draw()
    {
        var grid = GetParent<Node2D>();
        var lineColor = new Color(255, 255, 255);
        const float lineWidth = 2f;

        var gridSize = (Vector2)grid.Get("grid_size");
        var tileSize = (Vector2)grid.Get("tile_size");

        for (int x = 0; x <= (int)gridSize.X; x++)
        {
            float colPos = x * tileSize.X;
            float limit = gridSize.Y * tileSize.Y;
            DrawLine(new Vector2(colPos, 0), new Vector2(colPos, limit), lineColor, lineWidth);
        }

        for (int y = 0; y <= (int)gridSize.Y; y++)
        {
            float rowPos = y * tileSize.Y;
            float limit = gridSize.X * tileSize.X;
            DrawLine(new Vector2(0, rowPos), new Vector2(limit, rowPos), lineColor, lineWidth);
        }
    }
}
