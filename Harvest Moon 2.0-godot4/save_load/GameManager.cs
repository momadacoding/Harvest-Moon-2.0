using Godot;
using System.Globalization;

public partial class GameManager : Node
{
    public string game_type = "";
    public string file_name = "";
    public ulong start_time;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
    }

    public void save_game(string fileName)
    {
        var game = GetNode<Game>("/root/Game");
        var player = game.player;
        var saveFile = FileAccess.Open($"user://{fileName}.txt", FileAccess.ModeFlags.Write);

        var saveDictionary = new Godot.Collections.Dictionary
        {
            { "cropsDictionary", new Godot.Collections.Dictionary() },
            { "dirtDictionary", new Godot.Collections.Dictionary() },
            { "junkDictionary", new Godot.Collections.Dictionary() },
            { "textures", new Godot.Collections.Array() },
            { "labels", new Godot.Collections.Array() },
            { "stacked_items", new Godot.Collections.Dictionary() },
            { "IndicatorSlot", 0 },
            { "equippedItem", "null" },
            { "hotbar_textures", new Godot.Collections.Array() },
            { "hotbar_labels", new Godot.Collections.Array() },
            { "inventory_textures", new Godot.Collections.Array() },
            { "inventory_labels", new Godot.Collections.Array() },
            { "hotbar_IndicatorSlot", 0 },
            { "day", 0 },
            { "hour", 0 },
            { "armyTimeHour", 0 },
            { "minute", 0 },
            { "period", "null" },
            { "season", "null" },
            { "weather", "null" },
            { "raining", false },
            { "rain_one_shot", false },
            { "player_location", "null" },
            { "pos_x", 0.0f },
            { "pos_y", 0.0f },
            { "sleepTime", 0 },
            { "crop_number", 0 },
            { "holdingItem", false },
            { "holdTime", 0 },
            { "powerHold", false },
            { "do_it_once", false },
            { "lastAnimation", "null" },
            { "facingDirection", "null" },
            { "animationCommit", false },
            { "animation", "null" },
            { "animationFrame", 0 },
            { "teleport", false },
            { "speed", 0 },
            { "direction_x", 0.0f },
            { "direction_y", 0.0f },
            { "velocity_x", 0.0f },
            { "velocity_y", 0.0f },
            { "target_pos_x", 0.0f },
            { "target_pos_y", 0.0f },
            { "target_direction_x", 0.0f },
            { "target_direction_y", 0.0f },
            { "is_moving", false },
            { "stepLeft", false },
            { "stepRight", false },
            { "stepTime", 0 },
            { "flipped", false },
            { "holdEggplant", false },
            { "holdStrawberry", false },
            { "holdTurnip", false },
            { "energy", 0.0 },
            { "sound_dictionary", new Godot.Collections.Dictionary() },
            { "morningAlpha", 0.0f },
            { "afternoonAlpha", 0.0f },
            { "eveningAlpha", 0.0f },
            { "nightAlpha", 0.0f },
            { "TweenMorningOut", 0.0 },
            { "TweenAfternoonIn", 0.0 },
            { "TweenAfternoonOut", 0.0 },
            { "TweenEveningIn", 0.0 },
            { "TweenEveningOut", 0.0 },
            { "TweenNightIn", 0.0 }
        };

        var cropsDictionary = new Godot.Collections.Dictionary();
        var dirtDictionary = new Godot.Collections.Dictionary();
        var junkDictionary = new Godot.Collections.Dictionary();

        var crops = GetNode<TileMap>("/root/Game/Farm/Crops");
        var dirt = GetNode<TileMap>("/root/Game/Farm/Dirt");
        var junk = GetNode<TileMap>("/root/Game/Farm/Junk");

        foreach (Vector2I location in crops.GetUsedCells(0))
        {
            cropsDictionary[location] = GetTileSourceId(crops, location);
        }

        foreach (Vector2I location in dirt.GetUsedCells(0))
        {
            dirtDictionary[location] = GetTileSourceId(dirt, location);
        }

        foreach (Vector2I location in junk.GetUsedCells(0))
        {
            junkDictionary[location] = GetTileSourceId(junk, location);
        }

        saveDictionary["cropsDictionary"] = cropsDictionary;
        saveDictionary["dirtDictionary"] = dirtDictionary;
        saveDictionary["junkDictionary"] = junkDictionary;

        var inventory = player.GetNode<PlayerInventory>("UI/Inventory");
        var inventoryTextures = new Godot.Collections.Array();
        var inventoryLabels = new Godot.Collections.Array();

        foreach (var textureNode in inventory.textures_and_labels.Keys)
        {
            var textureRect = (TextureRect)textureNode;
            var label = (Label)inventory.textures_and_labels[textureNode];
            inventoryTextures.Add(ResourceBasename(textureRect.Texture));
            inventoryLabels.Add(string.IsNullOrEmpty(label.Text) ? "0" : label.Text);
        }

        saveDictionary["textures"] = inventoryTextures;
        saveDictionary["labels"] = inventoryLabels;
        saveDictionary["stacked_items"] = inventory.stacked_items;
        saveDictionary["IndicatorSlot"] = inventory.IndicatorSlot;
        saveDictionary["equippedItem"] = inventory.equippedItem;

        var hotbar = player.GetNode<Hotbar>("UI/Hotbar");
        var hotbarTextures = new Godot.Collections.Array();
        var hotbarLabels = new Godot.Collections.Array();

        foreach (var textureNode in hotbar.textures_and_labels.Keys)
        {
            var textureRect = (TextureRect)textureNode;
            var label = (Label)hotbar.textures_and_labels[textureNode];
            hotbarTextures.Add(ResourceBasename(textureRect.Texture));
            hotbarLabels.Add(string.IsNullOrEmpty(label.Text) ? "0" : label.Text);
        }

        var mirroredInventoryTextures = new Godot.Collections.Array();
        var mirroredInventoryLabels = new Godot.Collections.Array();

        foreach (var textureNode in hotbar.inventory_textures_and_labels.Keys)
        {
            var textureRect = (TextureRect)textureNode;
            var label = (Label)hotbar.inventory_textures_and_labels[textureNode];
            mirroredInventoryTextures.Add(ResourceBasename(textureRect.Texture));
            mirroredInventoryLabels.Add(string.IsNullOrEmpty(label.Text) ? "0" : label.Text);
        }

        saveDictionary["hotbar_textures"] = hotbarTextures;
        saveDictionary["hotbar_labels"] = hotbarLabels;
        saveDictionary["inventory_textures"] = mirroredInventoryTextures;
        saveDictionary["inventory_labels"] = mirroredInventoryLabels;
        saveDictionary["hotbar_IndicatorSlot"] = hotbar.IndicatorSlot;

        var timeManager = player.GetNode<DashboardTimeManager>("UI/Dashboard/TimeManager");
        saveDictionary["day"] = timeManager.day;
        saveDictionary["hour"] = timeManager.hour;
        saveDictionary["armyTimeHour"] = timeManager.armyTimeHour;
        saveDictionary["minute"] = timeManager.minute;
        saveDictionary["period"] = timeManager.period;
        saveDictionary["season"] = timeManager.season;

        var weather = player.GetNode<DashboardWeather>("UI/Dashboard/Weather");
        var rain = player.GetNode<GpuParticles2D>("Rain");
        saveDictionary["weather"] = ResourceBasename(weather.Texture);
        saveDictionary["raining"] = rain.Emitting;
        saveDictionary["rain_one_shot"] = rain.OneShot;

        var sprite = player.GetNode<AnimatedSprite2D>("Sprite2D");
        saveDictionary["player_location"] = game.player_location.Name.ToString();
        saveDictionary["pos_x"] = player.Position.X;
        saveDictionary["pos_y"] = player.Position.Y;
        saveDictionary["sleepTime"] = player.Get("sleepTime");
        saveDictionary["crop_number"] = player.Get("crop_number");
        saveDictionary["holdingItem"] = player.Get("holdingItem");
        saveDictionary["holdTime"] = player.Get("holdTime");
        saveDictionary["powerHold"] = player.Get("powerHold");
        saveDictionary["do_it_once"] = player.Get("do_it_once");
        saveDictionary["lastAnimation"] = player.Get("lastAnimation");
        saveDictionary["facingDirection"] = player.Get("facingDirection");
        saveDictionary["animationCommit"] = player.Get("animationCommit");
        saveDictionary["animation"] = sprite.Animation;
        saveDictionary["animationFrame"] = sprite.Frame;
        saveDictionary["teleport"] = player.Get("teleport");
        saveDictionary["speed"] = player.Get("speed");

        var direction = player.Get("direction").AsVector2();
        saveDictionary["direction_x"] = direction.X;
        saveDictionary["direction_y"] = direction.Y;
        saveDictionary["velocity_x"] = player.Velocity.X;
        saveDictionary["velocity_y"] = player.Velocity.Y;

        var targetPos = player.Get("target_pos").AsVector2();
        saveDictionary["target_pos_x"] = targetPos.X;
        saveDictionary["target_pos_y"] = targetPos.Y;

        var targetDirection = player.Get("target_direction").AsVector2();
        saveDictionary["target_direction_x"] = targetDirection.X;
        saveDictionary["target_direction_y"] = targetDirection.Y;

        saveDictionary["is_moving"] = player.Get("is_moving");
        saveDictionary["stepLeft"] = player.Get("stepLeft");
        saveDictionary["stepRight"] = player.Get("stepRight");
        saveDictionary["stepTime"] = player.Get("stepTime");
        saveDictionary["flipped"] = sprite.FlipH;
        saveDictionary["holdEggplant"] = player.GetNode<CanvasItem>("PickCrops/Eggplant").Visible;
        saveDictionary["holdStrawberry"] = player.GetNode<CanvasItem>("PickCrops/Strawberry").Visible;
        saveDictionary["holdTurnip"] = player.GetNode<CanvasItem>("PickCrops/Turnip").Visible;

        saveDictionary["energy"] = player.GetNode<Range>("UI/Energy Bar/Backdrop/Filled Bar").Value;

        var soundManager = GetNode<GameSoundManager>("/root/Game/Sound");
        var soundDictionary = new Godot.Collections.Dictionary();
        foreach (var soundPlayer in soundManager.sound_dictionary.Keys)
        {
            soundDictionary[soundPlayer.Name.ToString()] = soundManager.sound_dictionary[soundPlayer];
        }

        saveDictionary["sound_dictionary"] = soundDictionary;

        var shaders = GetNode<WorldShaders>("/root/Game/Shaders");
        saveDictionary["morningAlpha"] = shaders.GetNode<ColorRect>("Morning").Color.A;
        saveDictionary["afternoonAlpha"] = shaders.GetNode<ColorRect>("Afternoon").Color.A;
        saveDictionary["eveningAlpha"] = shaders.GetNode<ColorRect>("Evening").Color.A;
        saveDictionary["nightAlpha"] = shaders.GetNode<ColorRect>("Night").Color.A;

        var tweenProgress = (Godot.Collections.Dictionary)shaders.get_tween_progresses();
        saveDictionary["TweenMorningOut"] = tweenProgress["TweenMorningOut"];
        saveDictionary["TweenAfternoonIn"] = tweenProgress["TweenAfternoonIn"];
        saveDictionary["TweenAfternoonOut"] = tweenProgress["TweenAfternoonOut"];
        saveDictionary["TweenEveningIn"] = tweenProgress["TweenEveningIn"];
        saveDictionary["TweenEveningOut"] = tweenProgress["TweenEveningOut"];
        saveDictionary["TweenNightIn"] = tweenProgress["TweenNightIn"];

        saveFile.StoreLine(Json.Stringify(saveDictionary));
        saveFile.Close();
    }

    public void load_game(string file)
    {
        GetTree().ChangeSceneToFile("res://Game.tscn");
        SetPhysicsProcess(true);
        file_name = file;
        start_time = Time.GetTicksMsec();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Time.GetTicksMsec() < start_time + 10UL)
        {
            return;
        }

        GetTree().Paused = true;

        if (!FileAccess.FileExists($"user://{file_name}.txt"))
        {
            GD.Print("Error: Could not find file. Was it moved or deleted?");
            SetPhysicsProcess(false);
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://menus/main/MainMenu.tscn");
            return;
        }

        var game = GetNode<Game>("/root/Game");
        var player = game.player;

        var loadFile = FileAccess.Open($"user://{file_name}.txt", FileAccess.ModeFlags.Read);
        var dict = (Godot.Collections.Dictionary)Json.ParseString(loadFile.GetLine());

        var crops = GetNode<TileMap>("/root/Game/Farm/Crops");
        var dirt = GetNode<TileMap>("/root/Game/Farm/Dirt");
        var junk = GetNode<TileMap>("/root/Game/Farm/Junk");

        LoadTileDictionary(crops, (Godot.Collections.Dictionary)dict["cropsDictionary"]);
        LoadTileDictionary(dirt, (Godot.Collections.Dictionary)dict["dirtDictionary"]);
        LoadTileDictionary(junk, (Godot.Collections.Dictionary)dict["junkDictionary"]);

        var inventory = player.GetNode<PlayerInventory>("UI/Inventory");
        var textures = (Godot.Collections.Array)dict["textures"];
        var labels = (Godot.Collections.Array)dict["labels"];

        for (var i = 0; i < textures.Count; i++)
        {
            var textureName = ToStringValue(textures[i]);
            var labelText = ToStringValue(labels[i]);
            var textureRect = inventory.textures[i];
            var label = inventory.labels[i];

            textureRect.Texture = textureName == "none"
                ? null
                : GD.Load<Texture2D>($"res://ui/inventory/tools and items/{textureName}.png");
            label.Text = labelText == "0" ? "" : labelText;
        }

        inventory.stacked_items = (Godot.Collections.Dictionary)dict["stacked_items"];
        inventory.IndicatorSlot = ToInt(dict["IndicatorSlot"]);
        inventory.equippedItem = ToStringValue(dict["equippedItem"]);

        var hotbar = player.GetNode<Hotbar>("UI/Hotbar");
        var hotbarTextures = (Godot.Collections.Array)dict["hotbar_textures"];
        var hotbarLabels = (Godot.Collections.Array)dict["hotbar_labels"];

        for (var i = 0; i < hotbarTextures.Count; i++)
        {
            var textureName = ToStringValue(hotbarTextures[i]);
            var labelText = ToStringValue(hotbarLabels[i]);
            var textureRect = hotbar.textures[i];
            var label = hotbar.labels[i];

            textureRect.Texture = textureName == "none"
                ? null
                : GD.Load<Texture2D>($"res://ui/inventory/tools and items/{textureName}.png");
            label.Text = labelText == "0" ? "" : labelText;
        }

        var mirroredTextures = (Godot.Collections.Array)dict["inventory_textures"];
        var mirroredLabels = (Godot.Collections.Array)dict["inventory_labels"];

        for (var i = 0; i < mirroredTextures.Count; i++)
        {
            var textureName = ToStringValue(mirroredTextures[i]);
            var labelText = ToStringValue(mirroredLabels[i]);
            var textureRect = hotbar.inventory_textures[i];
            var label = hotbar.inventory_labels[i];

            textureRect.Texture = textureName == "none"
                ? null
                : GD.Load<Texture2D>($"res://ui/inventory/tools and items/{textureName}.png");
            label.Text = labelText == "0" ? "" : labelText;
        }

        hotbar.IndicatorSlot = ToInt(dict["hotbar_IndicatorSlot"]);
        hotbar._move_indicator();

        var timeManager = player.GetNode<DashboardTimeManager>("UI/Dashboard/TimeManager");
        timeManager.day = ToInt(dict["day"]);
        timeManager.hour = ToInt(dict["hour"]);
        timeManager.armyTimeHour = ToInt(dict["armyTimeHour"]);
        timeManager.minute = ToInt(dict["minute"]);
        timeManager.period = ToStringValue(dict["period"]);
        timeManager.season = ToStringValue(dict["season"]);
        timeManager.GetNode<Label>("Time").Text = $"{timeManager.hour}:{timeManager.minute:D2} {timeManager.period}";
        timeManager.GetNode<Label>("Season").Text = timeManager.season;
        timeManager.GetNode<Label>("Day").Text = $"Day {timeManager.day}";

        var weather = player.GetNode<DashboardWeather>("UI/Dashboard/Weather");
        var rain = player.GetNode<GpuParticles2D>("Rain");
        weather.Texture = GD.Load<Texture2D>($"res://ui/dashboard/weather/{ToStringValue(dict["weather"])}.png");
        rain.Emitting = ToBool(dict["raining"]);
        rain.OneShot = ToBool(dict["rain_one_shot"]);

        var playerLocation = ToStringValue(dict["player_location"]);
        if (playerLocation == "House")
        {
            game.farm_to_house();
        }
        else if (playerLocation == "Town")
        {
            game.farm_to_town();
        }

        player.Position = new Vector2(ToFloat(dict["pos_x"]), ToFloat(dict["pos_y"]));
        player.Set("sleepTime", ToInt(dict["sleepTime"]));
        player.Set("crop_number", ToInt(dict["crop_number"]));
        player.Set("holdingItem", ToBool(dict["holdingItem"]));
        player.Set("holdTime", ToInt(dict["holdTime"]));
        player.Set("powerHold", ToBool(dict["powerHold"]));
        player.Set("do_it_once", ToBool(dict["do_it_once"]));
        player.Set("lastAnimation", ToStringValue(dict["lastAnimation"]));
        player.Set("facingDirection", ToStringValue(dict["facingDirection"]));
        player.Set("animationCommit", ToBool(dict["animationCommit"]));

        var sprite = player.GetNode<AnimatedSprite2D>("Sprite2D");
        sprite.Animation = ToStringValue(dict["animation"]);
        sprite.Frame = ToInt(dict["animationFrame"]);

        player.Set("teleport", ToBool(dict["teleport"]));
        player.Set("speed", ToInt(dict["speed"]));
        player.Set("direction", new Vector2(ToFloat(dict["direction_x"]), ToFloat(dict["direction_y"])));
        player.Velocity = new Vector2(ToFloat(dict["velocity_x"]), ToFloat(dict["velocity_y"]));
        player.Set("target_pos", new Vector2(ToFloat(dict["target_pos_x"]), ToFloat(dict["target_pos_y"])));
        player.Set("target_direction", new Vector2(ToFloat(dict["target_direction_x"]), ToFloat(dict["target_direction_y"])));
        player.Set("is_moving", ToBool(dict["is_moving"]));
        player.Set("stepLeft", ToBool(dict["stepLeft"]));
        player.Set("stepRight", ToBool(dict["stepRight"]));
        player.Set("stepTime", ToInt(dict["stepTime"]));
        sprite.FlipH = ToBool(dict["flipped"]);
        player.GetNode<CanvasItem>("PickCrops/Eggplant").Visible = ToBool(dict["holdEggplant"]);
        player.GetNode<CanvasItem>("PickCrops/Strawberry").Visible = ToBool(dict["holdStrawberry"]);
        player.GetNode<CanvasItem>("PickCrops/Turnip").Visible = ToBool(dict["holdTurnip"]);

        player.GetNode<Range>("UI/Energy Bar/Backdrop/Filled Bar").Value = ToDouble(dict["energy"]);

        var soundManager = GetNode<GameSoundManager>("/root/Game/Sound");
        var soundDictionary = (Godot.Collections.Dictionary)dict["sound_dictionary"];
        foreach (Node soundNode in soundManager.GetChildren())
        {
            foreach (Node child in soundNode.GetChildren())
            {
                if (child is not AudioStreamPlayer sound)
                {
                    continue;
                }

                sound.Stop();
                if (soundDictionary.ContainsKey(sound.Name))
                {
                    sound.Play((float)ToDouble(soundDictionary[sound.Name]));
                }
            }
        }

        var shaders = GetNode<WorldShaders>("/root/Game/Shaders");
        shaders.restore_state(dict["morningAlpha"], dict["afternoonAlpha"], dict["eveningAlpha"], dict["nightAlpha"]);

        loadFile.Close();
        SetPhysicsProcess(false);
        GetTree().Paused = false;
    }

    public void set_game_type(string type)
    {
        game_type = type;
    }

    private static int GetTileSourceId(TileMap tileMap, Vector2I location)
    {
        return tileMap.GetCellSourceId(0, location);
    }

    private static void SetTile(TileMap tileMap, int x, int y, long tileId)
    {
        var coords = new Vector2I(x, y);
        if (tileId == -1)
        {
            tileMap.EraseCell(0, coords);
        }
        else
        {
            tileMap.SetCell(0, coords, (int)tileId);
        }
    }

    private static string ResourceBasename(Resource? resource)
    {
        return resource == null ? "none" : resource.ResourcePath.GetFile().GetBaseName();
    }

    private static void LoadTileDictionary(TileMap tileMap, Godot.Collections.Dictionary dictionary)
    {
        foreach (var key in dictionary.Keys)
        {
            var location = GetLocation(ToStringValue(key));
            SetTile(tileMap, location.X, location.Y, ToLong(dictionary[key]));
        }
    }

    private static Vector2I GetLocation(string location)
    {
        var trimmed = location.Trim('(', ')');
        var parts = trimmed.Split(',');
        return new Vector2I(
            int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
            int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture)
        );
    }

    private static string ToStringValue(object value)
    {
        return value?.ToString() ?? "";
    }

    private static int ToInt(object value)
    {
        if (value is int intValue)
        {
            return intValue;
        }

        return int.Parse(ToStringValue(value), CultureInfo.InvariantCulture);
    }

    private static long ToLong(object value)
    {
        if (value is long longValue)
        {
            return longValue;
        }

        return long.Parse(ToStringValue(value), CultureInfo.InvariantCulture);
    }

    private static float ToFloat(object value)
    {
        if (value is float floatValue)
        {
            return floatValue;
        }

        return float.Parse(ToStringValue(value), CultureInfo.InvariantCulture);
    }

    private static double ToDouble(object value)
    {
        if (value is double doubleValue)
        {
            return doubleValue;
        }

        return double.Parse(ToStringValue(value), CultureInfo.InvariantCulture);
    }

    private static bool ToBool(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }

        return bool.Parse(ToStringValue(value));
    }
}
