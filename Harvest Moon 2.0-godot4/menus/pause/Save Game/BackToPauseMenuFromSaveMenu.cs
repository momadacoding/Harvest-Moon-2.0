using Godot;

public partial class BackToPauseMenuFromSaveMenu : Control
{
    public void _on_Button_pressed()
    {
        var currentMenu = GetParent<CanvasItem>();
        currentMenu.Visible = false;

        var saveGame = GetParent().GetParent();
        var pauseMenu = saveGame.GetParent();
        pauseMenu.GetNode<CanvasItem>("Buttons").Visible = true;

        var callingNode = saveGame.Get("callingNode").AsString();
        if (callingNode == "Quit to Main Menu")
        {
            pauseMenu.GetNode<Control>("Buttons/Quit to Main Menu").GrabFocus();
        }
        else
        {
            pauseMenu.GetNode<Control>("Buttons/Quit to Desktop").GrabFocus();
        }
    }
}
