using Godot;

public partial class ShopMenu : Control
{
    private Game _game = null!;
    private CharacterBody2D _playerNode = null!;
    private TileMap _townGrid = null!;
    private Node2D _ui = null!;
    private PlayerInventory _inventory = null!;
    private Hotbar _hotbar = null!;
    private EnergyBar _energyBar = null!;
    private BuyMenu _buy = null!;
    private SellMenu _sell = null!;

    public override void _Ready()
    {
        _game = GetNode<Game>("/root/Game");
        _playerNode = GetNode<CharacterBody2D>("/root/Game/Farm/Player");
        _townGrid = GetNode<TileMap>("/root/Game/Town/DummyObject");
        _ui = GetNode<Node2D>("/root/Game/Farm/Player/UI");
        _inventory = GetNode<PlayerInventory>("/root/Game/Farm/Player/UI/Inventory");
        _hotbar = GetNode<Hotbar>("/root/Game/Farm/Player/UI/Hotbar");
        _energyBar = GetNode<EnergyBar>("/root/Game/Farm/Player/UI/Energy Bar");
        _buy = GetNode<BuyMenu>("Buy");
        _sell = GetNode<SellMenu>("Sell");

        SetTextureFilterNearest(this);

        GetNode<DashboardTimeManager>("/root/Game/Farm/Player/UI/Dashboard/TimeManager").sleep += _force_sleep;
    }

    private void SetTextureFilterNearest(Node node)
    {
        if (node is CanvasItem canvasItem)
            canvasItem.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        foreach (var child in node.GetChildren())
            SetTextureFilterNearest((Node)child);
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("E") &&
            _townGrid.LocalToMap(_playerNode.Position) == new Vector2I(27, 43) &&
            !_inventory.Visible && !Visible)
        {
            Visible = true;
            _hotbar.Visible = false;
            _energyBar.Visible = false;
            _ui.ZIndex = 5;
            Position = new Vector2(
                _game.player.Position.X - ((Size.X / 2) * Scale.X),
                _game.player.Position.Y - ((Size.Y / 2) * Scale.Y) + 13
            ) + _game.player_location.Position;
        }
        else if (Input.IsActionPressed("pause") && Visible)
        {
            _close_shop_menu();
        }
        else if (Input.IsActionPressed("Tab") && Visible)
        {
            switch_tabs();
        }
    }

    public void _close_shop_menu()
    {
        Visible = false;
        _buy.Visible = true;
        _sell.Visible = false;
        _hotbar.Visible = true;
        _energyBar.Visible = true;
        _ui.ZIndex = 3;
        _buy.reset_menu();
        _sell.reset_menu();
    }

    public void switch_tabs()
    {
        if (_buy.Visible)
        {
            _buy.Visible = false;
            _sell.Visible = true;
            _sell.update_sell_menu();
        }
        else
        {
            _buy.Visible = true;
            _sell.Visible = false;
            _buy.update_buy_menu();
        }
    }

    private void _force_sleep()
    {
        if (Visible)
            _close_shop_menu();
    }
}
