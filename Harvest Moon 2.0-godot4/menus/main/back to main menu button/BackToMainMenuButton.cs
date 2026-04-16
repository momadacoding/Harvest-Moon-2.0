using Godot;

public partial class BackToMainMenuButton : Control
{
    public void _on_Button_pressed()
    {
        GetNode<CanvasItem>("/root/MainMenu/Buttons").Visible = true;
        GetParent<CanvasItem>().Visible = false;

        var parentName = GetParent().Name.ToString();
        if (parentName == "Load Game")
        {
            GetNode<Control>("/root/MainMenu/Buttons/Load Game").GrabFocus();
        }
        else if (parentName == "Controls")
        {
            GetNode<Control>("/root/MainMenu/Buttons/Controls").GrabFocus();
        }
        else if (parentName == "Graphics")
        {
            GetNode<Control>("/root/MainMenu/Buttons/Graphics").GrabFocus();
        }
    }
}
