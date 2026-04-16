using Godot;
using System.Collections.Generic;

public partial class Hotbar : Control
{
    private Node _player = null!;
    private PlayerInventory _inventory = null!;
    private Control _shopMenu = null!;

    private TextureRect _indicator = null!;
    public int IndicatorSlot = 1;
    private static readonly Vector2 IndicatorBasePosition = new(50, 50);
    private const int HotbarItemSeparation = 111;

    public readonly Godot.Collections.Dictionary textures_and_labels = new();
    public readonly List<TextureRect> textures = new();
    public readonly List<Label> labels = new();

    public readonly Godot.Collections.Dictionary inventory_textures_and_labels = new();
    public readonly List<TextureRect> inventory_textures = new();
    public readonly List<Label> inventory_labels = new();

    public override void _Ready()
    {
        _player = GetParent().GetParent();
        _inventory = GetParent().GetNode<PlayerInventory>("Inventory");
        _shopMenu = GetNode<Control>("/root/Game/Menus/Shop Menu");
        _indicator = GetNode<TextureRect>("Equipped Item Indicator");

        foreach (var button in GetNode("Hotbar Grid Container").GetChildren())
        {
            ((Control)button).FocusMode = FocusModeEnum.None;
            var children = ((Node)button).GetChildren();
            var texture = (TextureRect)children[0];
            var label = (Label)children[1];
            textures.Add(texture);
            labels.Add(label);
            textures_and_labels[texture] = label;
        }

        foreach (var button in GetParent().GetNode("Inventory/Hotbar Grid Container").GetChildren())
        {
            var children = ((Node)button).GetChildren();
            var texture = (TextureRect)children[0];
            var label = (Label)children[1];
            inventory_textures.Add(texture);
            inventory_labels.Add(label);
            inventory_textures_and_labels[texture] = label;
        }
    }

    public void force_sleep()
    {
        Visible = true;
        mirror_inventory();
        _equip();
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("Tab") && GetPlayerSpeed() == 0 && !IsPlayerAnimationCommitted() &&
            !_inventory.DraggedItem.Visible && !_shopMenu.Visible)
        {
            if (Visible)
            {
                Visible = false;
            }
            else
            {
                Visible = true;
                mirror_inventory();
                _equip();
            }
        }
        else if (Input.IsActionPressed("pause") && !Visible && !_inventory.DraggedItem.Visible)
        {
            Visible = true;
        }
        else if (Visible && (Input.IsActionPressed("shift_left_arrow") || Input.IsActionPressed("scroll_down")))
        {
            IndicatorSlot = IndicatorSlot == 1 ? 10 : IndicatorSlot - 1;
            _move_indicator();
            _equip();
        }
        else if (Visible && (Input.IsActionPressed("shift_right_arrow") || Input.IsActionPressed("scroll_up")))
        {
            IndicatorSlot = IndicatorSlot == 10 ? 1 : IndicatorSlot + 1;
            _move_indicator();
            _equip();
        }
    }

    private int GetPlayerSpeed() => (int)_player.Get("speed");

    private bool IsPlayerAnimationCommitted() => (bool)_player.Get("animationCommit");

    public void _button_pressed(int number)
    {
        IndicatorSlot = number;
        _move_indicator();
        _equip();
    }

    public void _move_indicator()
    {
        _indicator.Position = new Vector2(IndicatorBasePosition.X + ((IndicatorSlot - 1) * HotbarItemSeparation), IndicatorBasePosition.Y);

        if (IndicatorSlot == 3)
        {
            _indicator.Scale = new Vector2(1.01f, 1.01f);
            var currentPosition = _indicator.Position;
            _indicator.Position = new Vector2(currentPosition.X - 1, currentPosition.Y);
        }
        else
        {
            _indicator.Scale = new Vector2(1, 1);
        }
    }

    private void _equip()
    {
        string itemToEquip;
        var currentTexture = textures[IndicatorSlot - 1].Texture;

        if (currentTexture != null)
        {
            itemToEquip = currentTexture.ResourcePath.GetFile().GetBaseName();
        }
        else
        {
            itemToEquip = "None";
        }

        _inventory.equip(itemToEquip);
    }

    public void mirror_inventory()
    {
        for (int i = 0; i < 10; i++)
        {
            textures[i].Texture = inventory_textures[i].Texture;
            labels[i].Text = inventory_labels[i].Text;
        }
    }
}
