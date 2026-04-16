using Godot;

public partial class DashboardWeather : TextureRect
{
    private Node _farm = null!;
    private Node _soundManager = null!;
    private GpuParticles2D _rain = null!;

    private Texture2D _sunny = null!;
    private Texture2D _cloudy = null!;
    private Texture2D _raining = null!;
    private Texture2D _night = null!;

    public override void _Ready()
    {
        _farm = GetNode("/root/Game/Farm");
        _soundManager = GetNode("/root/Game/Sound");
        _rain = GetParent().GetParent().GetParent().GetNode<GpuParticles2D>("Rain");

        _sunny = GD.Load<Texture2D>("res://ui/dashboard/weather/Sunny.png");
        _cloudy = GD.Load<Texture2D>("res://ui/dashboard/weather/Cloudy.png");
        _raining = GD.Load<Texture2D>("res://ui/dashboard/weather/Raining.png");
        _night = GD.Load<Texture2D>("res://ui/dashboard/weather/Night.png");
    }

    public void new_day()
    {
        var rainChance = GD.Randi() % 4 + 1;
        if (rainChance == 1)
        {
            if (!_rain.Emitting)
            {
                _rain.OneShot = false;
                _rain.Emitting = true;
                _soundManager.Call("play_music", "rain");
            }

            _farm.Call("simulate_rain");
            _set_weather("raining");
        }
        else
        {
            _rain.OneShot = true;
            _soundManager.Call("stop_music", "rain");
            _set_weather("sunny");
        }
    }

    public void set_weather_to_night()
    {
        if (!_rain.Emitting)
        {
            _set_weather("night");
        }
    }

    public void toggle_weather()
    {
        _rain.Visible = !_rain.Visible;
    }

    public void _set_weather(string new_weather)
    {
        if (new_weather == "sunny")
        {
            Texture = _sunny;
        }
        else if (new_weather == "cloudy")
        {
            Texture = _cloudy;
        }
        else if (new_weather == "raining")
        {
            Texture = _raining;
        }
        else if (new_weather == "night")
        {
            Texture = _night;
        }
    }
}
