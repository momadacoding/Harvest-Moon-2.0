using Godot;

public partial class BackToSaveMenu : Control
{
    public void _on_Button_pressed()
    {
        var currentMenu = GetParent<CanvasItem>();
        currentMenu.Visible = false;

        var saveGame = GetParent().GetParent();
        saveGame.GetNode<CanvasItem>("Save Menu").Visible = true;
        saveGame.GetNode<Control>("Save Menu/Save Options/New Save").GrabFocus();
    }
}
