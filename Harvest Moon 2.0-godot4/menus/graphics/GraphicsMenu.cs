using Godot;

public partial class GraphicsMenu : Control
{
    public void _on_Resolution_Drop_Down_item_selected(long id)
    {
        if (id == 0)
        {
            GetWindow().Size = new Vector2I(800, 600);
        }
        else if (id == 1)
        {
            GetWindow().Size = new Vector2I(1024, 576);
        }
        else if (id == 2)
        {
            GetWindow().Size = new Vector2I(1280, 800);
        }
        else if (id == 3)
        {
            GetWindow().Size = new Vector2I(1366, 768);
        }
        else if (id == 4)
        {
            GetWindow().Size = new Vector2I(1920, 1080);
        }
        else if (id == 5)
        {
            GetWindow().Size = new Vector2I(3840, 2160);
        }
    }

    public void _on_Window_Drop_Down_item_selected(long id)
    {
        if (id == 0)
        {
            GetWindow().Mode = Window.ModeEnum.Windowed;
            GetWindow().Borderless = false;
        }
        else if (id == 1)
        {
            GetWindow().Mode = Window.ModeEnum.ExclusiveFullscreen;
            GetWindow().Borderless = true;
        }
        else if (id == 2)
        {
            GetWindow().Mode = Window.ModeEnum.ExclusiveFullscreen;
            GetWindow().Borderless = false;
        }
    }
}
