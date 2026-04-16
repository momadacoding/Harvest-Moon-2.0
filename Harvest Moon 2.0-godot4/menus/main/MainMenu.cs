using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Control>("Buttons/New Game").GrabFocus();
	}

	public void _on_New_Game_pressed()
	{
		GetNode<CanvasItem>("Buttons").Visible = false;
		GetNode<CanvasItem>("Dialogue").Visible = true;
		GetNode<Control>("Dialogue/To the game button").GrabFocus();
		GetNode<Node>("/root/GameManager").Call("set_game_type", "new");
		GD.Print("MainMenu: New Game pressed. Buttons hidden, Dialogue shown. Waiting for 'To the game' button.");
	}

	public void _on_Load_Game_pressed()
	{
		GetNode<CanvasItem>("Buttons").Visible = false;
		var loadGame = GetNode<Node>("Load Game");
		((CanvasItem)loadGame).Visible = true;
		loadGame.Call("list_saves");
	}

	public void _on_Controls_pressed()
	{
		GetNode<CanvasItem>("Buttons").Visible = false;
		GetNode<CanvasItem>("Controls").Visible = true;
		GetNode<Control>("Controls/Back to Main Menu/Container/Button").GrabFocus();
	}

	public void _on_Graphics_pressed()
	{
		GetNode<CanvasItem>("Buttons").Visible = false;
		GetNode<CanvasItem>("Graphics").Visible = true;
		GetNode<Control>("Graphics/Graphics Container/Resolution Container/Resolution Drop Down").GrabFocus();
	}

	public void _on_Quit_to_Desktop_pressed()
	{
		GetTree().Quit();
	}
}
