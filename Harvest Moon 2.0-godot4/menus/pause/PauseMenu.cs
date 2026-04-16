using Godot;

public partial class PauseMenu : Control
{
    private Game _game = null!;
    private GameSoundManager _soundManager = null!;
    private Control _shopMenu = null!;
    private Node _gameManager = null!;

    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Game");
        _soundManager = GetNode<GameSoundManager>("/root/Game/Sound");
        _shopMenu = GetParent().GetNode<Control>("Shop Menu");
        _gameManager = GetNode<Node>("/root/GameManager");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            if (Visible)
            {
                _resume_game();
            }
            else if (!Visible && !_shopMenu.Visible &&
                     !_game.player.GetNode<PlayerInventory>("UI/Inventory").Visible)
            {
                _pause_game();
            }
        }
    }

    public void _on_Resume_pressed()
    {
        _resume_game();
    }

    public void _on_Controls_pressed()
    {
        GetNode<CanvasItem>("Buttons").Visible = false;
        GetNode<CanvasItem>("Controls").Visible = true;
        GetNode<Control>("Controls/Back to Pause Menu/Container/Button").GrabFocus();
    }

    public void _on_Quit_to_Main_Menu_pressed()
    {
        GetNode<CanvasItem>("Buttons").Visible = false;
        GetNode<CanvasItem>("Save Game/Save Menu").Visible = true;

        var saveGame = GetNode<SaveGameMenu>("Save Game");

        if ((string)_gameManager.Get("game_type") == "new")
        {
            GetNode<CanvasItem>("Save Game/Save Menu/Save Options/Overwrite").Visible = false;
            GetNode<Control>("Save Game/Save Menu/Save Options/New Save").GrabFocus();
            GetNode<Control>("Save Game/Save Menu/Save Options").Position = new Vector2(160, 113);
        }
        else
        {
            GetNode<CanvasItem>("Save Game/Save Menu/Save Options/Overwrite").Visible = true;
            GetNode<Control>("Save Game/Save Menu/Save Options/Overwrite").GrabFocus();
            GetNode<Control>("Save Game/Save Menu/Save Options").Position = new Vector2(160, 101);
        }

        saveGame.callingNode = "Quit to Main Menu";
    }

    public void _on_Quit_to_Desktop_pressed()
    {
        GetNode<CanvasItem>("Buttons").Visible = false;
        GetNode<CanvasItem>("Save Game/Save Menu").Visible = true;

        var saveGame = GetNode<SaveGameMenu>("Save Game");

        if ((string)_gameManager.Get("game_type") == "new")
        {
            GetNode<CanvasItem>("Save Game/Save Menu/Save Options/Overwrite").Visible = false;
            GetNode<Control>("Save Game/Save Menu/Save Options/New Save").GrabFocus();
            GetNode<Control>("Save Game/Save Menu/Save Options").Position = new Vector2(160, 113);
        }
        else
        {
            GetNode<CanvasItem>("Save Game/Save Menu/Save Options/Overwrite").Visible = true;
            GetNode<Control>("Save Game/Save Menu/Save Options/Overwrite").GrabFocus();
            GetNode<Control>("Save Game/Save Menu/Save Options").Position = new Vector2(160, 101);
        }

        saveGame.callingNode = "Quit to Desktop";
    }

    private void _resume_game()
    {
        GetTree().Paused = false;
        Visible = false;
        _soundManager.resume_all_sounds();
    }

    private void _pause_game()
    {
        GetTree().Paused = true;
        Position = new Vector2(
            _game.player.Position.X - Size.X / 2,
            _game.player.Position.Y - Size.Y / 2
        ) + _game.player_location.Position;
        GetNode<Control>("Buttons/Resume").GrabFocus();
        Visible = true;
        _soundManager.pause_all_sounds();
    }
}
