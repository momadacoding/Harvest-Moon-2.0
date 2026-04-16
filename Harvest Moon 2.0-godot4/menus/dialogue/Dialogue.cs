using Godot;

public partial class Dialogue : Panel
{
    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        Size = GetViewportRect().Size;

        var intro = GetNode<RichTextLabel>("Game Intro");
        var button = GetNode<Button>("To the game button");
        GD.Print($"Dialogue: Ready. Size={Size}, IntroSize={intro.Size}, ButtonPosition={button.Position}, ButtonSize={button.Size}");
    }

    public void _on_To_the_game_button_pressed()
    {
        GD.Print("Dialogue: 'To the game' pressed. Attempting to change scene to res://Game.tscn");
        var error = GetTree().ChangeSceneToFile("res://Game.tscn");
        if (error != Error.Ok)
        {
            GD.PushError($"Dialogue: Failed to change scene to res://Game.tscn. Error={error}");
        }
    }
}
