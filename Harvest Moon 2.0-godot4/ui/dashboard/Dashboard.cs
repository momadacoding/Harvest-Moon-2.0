using Godot;

public partial class Dashboard : TextureRect
{
    public void new_day()
    {
        GetNode<Node>("TimeManager").Call("new_day");
        GetNode<Node>("Weather").Call("new_day");
    }
}
