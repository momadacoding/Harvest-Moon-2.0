using Godot;

public partial class BackToPauseMenuButton : Control
{
    public void _on_Button_pressed()
    {
        var currentMenu = GetParent<CanvasItem>();
        currentMenu.Visible = false;

        var pauseMenu = currentMenu.GetParent();
        pauseMenu.GetNode<CanvasItem>("Buttons").Visible = true;

        if (currentMenu.Name.ToString() == "Controls")
        {
            pauseMenu.GetNode<Control>("Buttons/Controls").GrabFocus();
        }
    }
}
