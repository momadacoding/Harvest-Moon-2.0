using Godot;

public partial class LoadGameMenu : Control
{
    private PackedScene _buttonScene = null!;
    private ScrollContainer _saves = null!;
    private VBoxContainer _buttons = null!;

    public override void _Ready()
    {
        _buttonScene = GD.Load<PackedScene>("res://menus/load/Load Save Button.tscn");
        _saves = GetNode<ScrollContainer>("Saves");
        _buttons = GetNode<VBoxContainer>("Saves/Buttons");
    }

    public void list_saves()
    {
        foreach (Node child in _buttons.GetChildren())
        {
            child.Free();
        }

        var dir = DirAccess.Open("user://");
        if (dir is null)
        {
            return;
        }

        dir.ListDirBegin();

        var index = 0;

        while (true)
        {
            var fileName = dir.GetNext();
            if (fileName == string.Empty)
            {
                break;
            }

            var instancedButton = _buttonScene.Instantiate<Button>();
            instancedButton.Text = fileName[..^4];
            var buttonIndex = index;
            instancedButton.Pressed += () => _on_button_pressed(buttonIndex);
            _buttons.AddChild(instancedButton);
            index += 1;
        }

        dir.ListDirEnd();

        var numButtons = _buttons.GetChildCount();

        if (numButtons == 0)
        {
            var instancedButton = _buttonScene.Instantiate<Button>();
            instancedButton.Text = "You currently have no saves";
            _buttons.AddChild(instancedButton);

            _saves.Size = new Vector2(202, 32);
            _saves.Position = new Vector2(380, 336);
            GetNode<Control>("Back to Main Menu/Container/Button").GrabFocus();
            return;
        }

        var buttonsLength = ((Control)_buttons.GetChild(0)).Size.X;
        var buttonsHeight = (numButtons * 20) + ((numButtons - 1) * 4);

        _buttons.Size = new Vector2(buttonsLength, buttonsHeight);
        _saves.Set("scroll_horizontal_enabled", false);

        if (numButtons <= 10)
        {
            _saves.Set("scroll_vertical_enabled", false);
            _saves.Size = new Vector2(buttonsLength, buttonsHeight);
        }
        else
        {
            _saves.Set("scroll_vertical_enabled", true);
            _saves.Size = new Vector2(buttonsLength + 12, 236);
        }

        _saves.Position = new Vector2(
            (1366 / 2.0f) - ((_saves.Scale.X * _saves.Size.X) / 2.0f),
            (768 / 2.0f) - ((_saves.Scale.Y * _saves.Size.Y) / 2.0f)
        );

        ((Control)_buttons.GetChild(0)).GrabFocus();
    }

    public void _on_button_pressed(int index)
    {
        GetNode<Node>("/root/GameManager").Call("load_game", ((Button)_buttons.GetChild(index)).Text);
        GetNode<Node>("/root/GameManager").Call("set_game_type", "loaded");
    }
}
