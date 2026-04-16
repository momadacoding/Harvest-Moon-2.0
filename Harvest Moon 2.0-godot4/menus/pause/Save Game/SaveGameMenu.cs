using Godot;

public partial class SaveGameMenu : Control
{
    public string callingNode = "";
    private Node _gameManager = null!;

    public override void _Ready()
    {
        _gameManager = GetNode<Node>("/root/GameManager");
    }

    public void _on_Quit_without_Saving_pressed()
    {
        quit_game();
    }

    public void _on_New_Save_pressed()
    {
        GetNode<CanvasItem>("Save Menu").Visible = false;
        GetNode<CanvasItem>("New Save Menu").Visible = true;
        GetNode<Control>("New Save Menu/New Save").GrabFocus();
    }

    public void _on_New_Save_text_entered(string save_file)
    {
        if (string.IsNullOrEmpty(save_file))
            return;
        _gameManager.Call("save_game", save_file);
        quit_game();
    }

    public void quit_game()
    {
        if (callingNode == "Quit to Main Menu")
        {
            GetTree().ChangeSceneToFile("res://menus/main/MainMenu.tscn");
            GetTree().Paused = false;
        }
        else
        {
            GetTree().Quit();
        }
    }

    public void _on_Overwrite_pressed()
    {
        _gameManager.Call("save_game", (string)_gameManager.Get("file_name"));
        quit_game();
    }
}
