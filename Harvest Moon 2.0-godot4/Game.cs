using Godot;

public partial class Game : Node
{
    private static readonly Vector2 FarmToHouseSpawn = new(4, 7);
    private static readonly Vector2 HouseToFarmSpawn = new(19, 9);
    private static readonly Vector2 FarmToTownSpawn = new(37, 92);
    private static readonly Vector2 TownToFarmSpawn = new(19, 0);
    private static readonly Vector2 SleepSpawn = new(7, 4);

    private Node2D _farm = null!;
    private Node2D _house = null!;
    private Node2D _town = null!;

    private TileMap _farmMap = null!;
    private TileMap _houseMap = null!;
    private TileMap _townMap = null!;

    private Node _shaders = null!;
    private Node _weather = null!;
    private GameSoundManager _soundManager = null!;
    private Dashboard _dashboard = null!;

    public Vector2 tile_size { get; } = new(32, 32);
    public Vector2 half_tile_size => tile_size / 2.0f;

    public CharacterBody2D player { get; private set; } = null!;
    public Node2D player_location { get; private set; } = null!;

    public override void _Ready()
    {
        _farm = GetNode<Node2D>("Farm");
        _house = GetNode<Node2D>("House");
        _town = GetNode<Node2D>("Town");

        _farmMap = GetNode<TileMap>("Farm/Objects1");
        _houseMap = GetNode<TileMap>("House/Objects");
        _townMap = GetNode<TileMap>("Town/DummyObject");

        player = GetNode<CharacterBody2D>("Farm/Player");
        player_location = _farm;

        _shaders = GetNode("Shaders");
        _weather = GetNode("Farm/Player/UI/Dashboard/Weather");
        _soundManager = GetNode<GameSoundManager>("Sound");
        _dashboard = GetNode<Dashboard>("Farm/Player/UI/Dashboard");

        var count = CountNodes(this);
        GD.Print($"The Count is:{count}");
    }

    public void new_day()
    {
        _shaders.Call("new_day");
        _dashboard.new_day();
    }

    public void farm_to_house()
    {
        MovePlayer(_farm, _house, _houseMap, FarmToHouseSpawn);
        _shaders.Call("toggle_shaders");
        _weather.Call("toggle_weather");
        _soundManager.stop_music("farm");
        _soundManager.play_music("house");
        _soundManager.set_music_volume("rain", -10);
    }

    public void house_to_farm()
    {
        MovePlayer(_house, _farm, _farmMap, HouseToFarmSpawn);
        _shaders.Call("toggle_shaders");
        _weather.Call("toggle_weather");
        _soundManager.stop_music("house");
        _soundManager.play_music("farm");
        _soundManager.set_music_volume("rain", 0);
    }

    public void farm_to_town()
    {
        MovePlayer(_farm, _town, _townMap, FarmToTownSpawn);
        _soundManager.stop_music("farm");
        _soundManager.play_music("town");
    }

    public void town_to_farm()
    {
        MovePlayer(_town, _farm, _farmMap, TownToFarmSpawn);
        _soundManager.stop_music("town");
        _soundManager.play_music("farm");
    }

    public void teleport_player_to_bed()
    {
        MovePlayer(player_location, _house, _houseMap, SleepSpawn);
        _shaders.Call("toggle_shaders");
        _weather.Call("toggle_weather");
        _soundManager.set_music_volume("rain", -10);
    }

    public void house_to_sleep()
    {
        player.Position = _houseMap.MapToLocal((Vector2I)SleepSpawn) + half_tile_size;
    }

    private static int CountNodes(Node root)
    {
        var nodes = new Godot.Collections.Array<Node> { root };
        var count = 0;

        while (nodes.Count > 0)
        {
            var current = nodes[0];
            nodes.RemoveAt(0);
            count += 1;

            foreach (Node child in current.GetChildren())
            {
                nodes.Add(child);
            }
        }

        return count;
    }

    private void MovePlayer(Node2D fromArea, Node2D toArea, TileMap targetMap, Vector2 spawnCell)
    {
        fromArea.RemoveChild(player);
        toArea.AddChild(player);
        player.Owner = toArea;
        player.Position = targetMap.MapToLocal((Vector2I)spawnCell) + half_tile_size;
        player_location = toArea;
    }
}
