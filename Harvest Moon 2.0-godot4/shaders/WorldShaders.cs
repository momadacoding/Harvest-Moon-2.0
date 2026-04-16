using Godot;
using System.Collections.Generic;

public partial class WorldShaders : Node2D
{
    private static readonly Vector2 ViewportSize = new(480, 270);

    private Game _game = null!;
    private ColorRect _morning = null!;
    private ColorRect _afternoon = null!;
    private ColorRect _evening = null!;
    private ColorRect _night = null!;

    private readonly Dictionary<string, Tween> _activeTweens = new();
    private double? _tweenerDuration;

    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Game");
        _morning = GetNode<ColorRect>("Morning");
        _afternoon = GetNode<ColorRect>("Afternoon");
        _evening = GetNode<ColorRect>("Evening");
        _night = GetNode<ColorRect>("Night");

        fade_in_shader("afternoon");
    }

    public override void _Process(double delta)
    {
        Position = new Vector2(
            _game.player.Position.X - ViewportSize.X / 2.0f,
            _game.player.Position.Y - ViewportSize.Y / 2.0f
        ) + _game.player_location.Position;
    }

    public void new_day()
    {
        _reset_tweeners();
        fade_in_shader("afternoon");
    }

    public void fade_in_shader(string time)
    {
        if (_tweenerDuration is null)
        {
            var timer = GetNode<Timer>("/root/Game/Farm/Player/UI/Dashboard/TimeManager/Time/Timer");
            _tweenerDuration = timer.WaitTime * 300.0;
        }

        if (time == "afternoon")
        {
            _tween_shader("TweenMorningOut", _morning, new Color(0.79f, 0.79f, 0.32f, 0.35f), new Color(0.79f, 0.79f, 0.32f, 0.0f), Tween.EaseType.Out);
            _tween_shader("TweenAfternoonIn", _afternoon, new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 0f), Tween.EaseType.Out);
        }
        else if (time == "evening")
        {
            _tween_shader("TweenAfternoonOut", _afternoon, new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 0f), Tween.EaseType.In);
            _tween_shader("TweenEveningIn", _evening, new Color(1f, 0.33f, 0f, 0f), new Color(1f, 0.33f, 0f, 0.25f), Tween.EaseType.In);
        }
        else if (time == "night")
        {
            _tween_shader("TweenEveningOut", _evening, new Color(1f, 0.33f, 0f, 0.25f), new Color(1f, 0.33f, 0f, 0f), Tween.EaseType.In);
            _tween_shader("TweenNightIn", _night, new Color(0.05f, 0.09f, 0.15f, 0f), new Color(0.05f, 0.09f, 0.15f, 0.75f), Tween.EaseType.In);
        }
    }

    public void restore_state(Variant morning_alpha, Variant afternoon_alpha, Variant evening_alpha, Variant night_alpha)
    {
        _reset_tweeners();
        _morning.Color = new Color(0.79f, 0.79f, 0.32f, (float)morning_alpha.AsDouble());
        _afternoon.Color = new Color(1f, 1f, 1f, (float)afternoon_alpha.AsDouble());
        _evening.Color = new Color(1f, 0.33f, 0f, (float)evening_alpha.AsDouble());
        _night.Color = new Color(0.05f, 0.09f, 0.15f, (float)night_alpha.AsDouble());
    }

    public Godot.Collections.Dictionary get_tween_progresses()
    {
        return new Godot.Collections.Dictionary
        {
            { "TweenMorningOut", 0.0 },
            { "TweenAfternoonIn", 0.0 },
            { "TweenAfternoonOut", 0.0 },
            { "TweenEveningIn", 0.0 },
            { "TweenEveningOut", 0.0 },
            { "TweenNightIn", 0.0 }
        };
    }

    public void toggle_shaders()
    {
        Visible = !Visible;
    }

    private void _reset_tweeners()
    {
        foreach (var tweenName in _activeTweens.Keys)
        {
            var tween = _activeTweens[tweenName];
            if (GodotObject.IsInstanceValid(tween))
            {
                tween.Kill();
            }
        }

        _activeTweens.Clear();

        _morning.Color = new Color(0.79f, 0.79f, 0.32f, 0.35f);
        _afternoon.Color = new Color(1f, 1f, 1f, 0f);
        _evening.Color = new Color(1f, 0.33f, 0f, 0f);
        _night.Color = new Color(0.05f, 0.09f, 0.15f, 0f);
    }

    private void _tween_shader(string tweenName, ColorRect canvasItem, Color fromColor, Color toColor, Tween.EaseType ease)
    {
        if (_activeTweens.TryGetValue(tweenName, out var existingTween) && GodotObject.IsInstanceValid(existingTween))
        {
            existingTween.Kill();
        }

        canvasItem.Color = fromColor;
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.SetEase(ease);
        tween.TweenProperty(canvasItem, "color", toColor, _tweenerDuration ?? 0.0);
        _activeTweens[tweenName] = tween;
    }
}
