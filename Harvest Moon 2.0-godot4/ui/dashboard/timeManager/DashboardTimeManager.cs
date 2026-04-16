using Godot;

public partial class DashboardTimeManager : VBoxContainer
{
    [Signal]
    public delegate void sleepEventHandler();

    private Node _shaders = null!;
    private Node _weather = null!;
    private Label _time = null!;
    private Label _season = null!;
    private Label _dayLabel = null!;

    public int day { get; set; } = 1;
    public int hour { get; set; } = 6;
    public int armyTimeHour { get; set; } = 6;
    public int minute { get; set; }
    public string period { get; set; } = "AM";
    public string season { get; set; } = "Spring";

    public override void _Ready()
    {
        _shaders = GetNode("/root/Game/Shaders");
        _weather = GetParent().GetNode("Weather");
        _time = GetNode<Label>("Time");
        _season = GetNode<Label>("Season");
        _dayLabel = GetNode<Label>("Day");
    }

    public void new_day()
    {
        day += 1;
        hour = 6;
        armyTimeHour = 6;
        minute = 0;
        period = "AM";

        if (day % 61 <= 15)
        {
            season = "Spring";
        }
        else if (day % 61 <= 30)
        {
            season = "Summer";
        }
        else if (day % 61 <= 45)
        {
            season = "Fall";
        }
        else if (day % 61 <= 60)
        {
            season = "Winter";
        }

        RedrawLabels();
    }

    public void _on_Timer_timeout()
    {
        minute += 1;

        if (minute == 60)
        {
            minute = 0;
            hour += 1;
            armyTimeHour += 1;
        }

        if (hour == 12 && minute == 0)
        {
            period = period == "AM" ? "PM" : "AM";
        }

        if (hour == 13)
        {
            hour = 1;
        }

        _time.Text = $"{hour}:{minute:D2} {period}";

        if (armyTimeHour == 6 && minute == 0)
        {
            _shaders.Call("fade_in_shader", "afternoon");
        }
        else if (armyTimeHour == 12 && minute == 0)
        {
            _shaders.Call("fade_in_shader", "evening");
        }
        else if (armyTimeHour == 17 && minute == 0)
        {
            _shaders.Call("fade_in_shader", "night");
        }

        if (armyTimeHour == 20)
        {
            _weather.Call("set_weather_to_night");
        }

        if (armyTimeHour == 23 && minute == 0)
        {
            EmitSignal("sleep");
        }
    }

    private void RedrawLabels()
    {
        _time.Text = $"{hour}:{minute:D2} {period}";
        _season.Text = season;
        _dayLabel.Text = $"Day {day}";
    }
}
