using Godot;

public interface IGameArea
{
    Vector2 tile_size { get; }
    Vector2 half_tile_size { get; }
    bool teleport(Vector2 position);
    bool is_cell_vacant(Vector2 pos, Vector2 direction);
    Vector2 update_child_pos(CharacterBody2D childNode);
}
