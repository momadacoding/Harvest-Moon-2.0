using Godot;
using System.Collections.Generic;

public partial class PlayerInventory : Control
{
    private Node _player = null!;
    private Control _shopMenu = null!;

    public TextureRect DraggedItem = null!;
    private Label _draggedItemLabel = null!;
    private TextureRect _indicator = null!;

    public int IndicatorSlot = 1;
    private static readonly Vector2 IndicatorBasePosition = new(50, 394);
    private const int InventoryItemSeparation = 111;

    private Hotbar _hotbar = null!;
    private TextureRect _hotbarIndicator = null!;

    private static readonly Vector2 InventoryOffset = new(50, 50);
    private static readonly Vector2 SlotSize = new(100, 100);
    private static readonly Vector2 BorderSize = new(11, 11);
    private static readonly Vector2 InventoryBounds = new(1099, 443);

    private int _savedSlot;

    public readonly Godot.Collections.Dictionary textures_and_labels = new();
    public readonly List<TextureRect> textures = new();
    public readonly List<Label> labels = new();

    public Godot.Collections.Dictionary stacked_items = new()
    {
        { "StrawberrySeeds", 18 },
        { "Gold", 10 }
    };
    private static readonly string[] StackableItems = { "Strawberry", "Turnip", "Eggplant", "StrawberrySeeds", "TurnipSeeds", "EggplantSeeds", "Gold" };

    public string equippedItem = "Watering Can";

    private bool IsStackable(string item)
    {
        foreach (var s in StackableItems)
            if (s == item) return true;
        return false;
    }

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        _player = GetParent().GetParent();
        _shopMenu = GetNode<Control>("/root/Game/Menus/Shop Menu");

        DraggedItem = GetNode<TextureRect>("Dragged Item");
        _draggedItemLabel = GetNode<Label>("Dragged Item/Dragged Item Label");
        _indicator = GetNode<TextureRect>("Equipped Item Indicator");

        _hotbar = GetParent().GetNode<Hotbar>("Hotbar");
        _hotbarIndicator = GetParent().GetNode<TextureRect>("Hotbar/Equipped Item Indicator");

        GetNode<DashboardTimeManager>("/root/Game/Farm/Player/UI/Dashboard/TimeManager").sleep += _force_sleep;

        foreach (var button in GetNode("Inventory Grid Container").GetChildren())
        {
            var children = ((Node)button).GetChildren();
            var texture = (TextureRect)children[0];
            var label = (Label)children[1];
            textures.Add(texture);
            labels.Add(label);
            textures_and_labels[texture] = label;
        }

        foreach (var button in GetNode("Hotbar Grid Container").GetChildren())
        {
            var children = ((Node)button).GetChildren();
            var texture = (TextureRect)children[0];
            var label = (Label)children[1];
            textures.Add(texture);
            labels.Add(label);
            textures_and_labels[texture] = label;
        }
    }

    private void _force_sleep()
    {
        Visible = false;
        if (DraggedItem.Visible)
            _drop_dragged_item();
        _hotbar.force_sleep();
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionPressed("Tab") && GetPlayerSpeed() == 0 && !IsPlayerAnimationCommitted() &&
            !DraggedItem.Visible && !_shopMenu.Visible)
        {
            if (Visible)
            {
                Visible = false;
            }
            else
            {
                Visible = true;
                _indicator.Position = new Vector2(_hotbarIndicator.Position.X, 394);
                _indicator.Scale = _hotbarIndicator.Scale;
            }
        }
        else if (Input.IsActionPressed("pause") && Visible && !DraggedItem.Visible)
        {
            Visible = false;
        }
    }

    private int GetPlayerSpeed() => (int)_player.Get("speed");

    private bool IsPlayerAnimationCommitted() => (bool)_player.Get("animationCommit");

    public override void _PhysicsProcess(double delta)
    {
        DraggedItem.Position = GetLocalMousePosition() - InventoryOffset;

        if (!Input.IsActionPressed("left_click"))
            _drop_dragged_item();
    }

    private void _drop_dragged_item()
    {
        SetPhysicsProcess(false);

        var absoluteLocation = GetLocalMousePosition() - InventoryOffset;

        if (absoluteLocation.Y > 333)
            absoluteLocation.Y -= 11;

        var xOffsetLocation = ((int)absoluteLocation.X % 100) - (((int)(absoluteLocation.X / 100) - 1) * 11);
        var yOffsetLocation = ((int)absoluteLocation.Y % 100) - (((int)(absoluteLocation.Y / 100) - 1) * 11);

        if (absoluteLocation.X > InventoryBounds.X || absoluteLocation.Y > InventoryBounds.Y ||
            absoluteLocation.X < 0 || absoluteLocation.Y < 0 ||
            (xOffsetLocation >= 1 && xOffsetLocation <= 11) || absoluteLocation.X == 0 ||
            (yOffsetLocation >= 1 && yOffsetLocation <= 11) || absoluteLocation.Y == 0)
        {
            textures[_savedSlot - 1].Texture = DraggedItem.Texture;
            labels[_savedSlot - 1].Text = _draggedItemLabel.Text;
        }
        else
        {
            var mouseLocation = ((GetLocalMousePosition() - InventoryOffset) / new Vector2(111, 111)).Floor();
            int slotLocation = (int)(mouseLocation.X + 1 + (mouseLocation.Y * 10));

            if (textures[slotLocation - 1].Texture == null)
            {
                textures[slotLocation - 1].Texture = DraggedItem.Texture;
                labels[slotLocation - 1].Text = _draggedItemLabel.Text;
            }
            else
            {
                textures[_savedSlot - 1].Texture = textures[slotLocation - 1].Texture;
                labels[_savedSlot - 1].Text = labels[slotLocation - 1].Text;
                textures[slotLocation - 1].Texture = DraggedItem.Texture;
                labels[slotLocation - 1].Text = _draggedItemLabel.Text;
            }
        }

        DraggedItem.Visible = false;
    }

    public bool is_equipped(string item) => item == equippedItem;

    private bool _contains(string item)
    {
        foreach (var textureNode in textures)
        {
            var texture = textureNode.Texture;
            if (texture != null)
            {
                var path = texture.ResourcePath.GetFile().GetBaseName();
                if (item == path)
                    return true;
            }
        }
        return false;
    }

    public int get_amount(string item)
    {
        if (stacked_items.ContainsKey(item))
            return (int)stacked_items[item];

        int count = 0;
        foreach (var textureNode in textures)
        {
            var texture = textureNode.Texture;
            if (texture != null)
            {
                var path = texture.ResourcePath.GetFile().GetBaseName();
                if (item == path)
                    count++;
            }
        }
        return count;
    }

    public void equip(string item) => equippedItem = item;

    public void add(string item, int amount = 1)
    {
        if (IsStackable(item))
        {
            if (_contains(item))
            {
                int number = (int)stacked_items[item];
                stacked_items[item] = number + amount;

                for (int i = 0; i < textures.Count; i++)
                {
                    var texture = textures[i].Texture;
                    if (texture != null && item == texture.ResourcePath.GetFile().GetBaseName())
                    {
                        labels[i].Text = (number + amount).ToString();
                        break;
                    }
                }

                _hotbar.mirror_inventory();
            }
            else
            {
                stacked_items[item] = amount;
                for (int i = 0; i < textures.Count; i++)
                {
                    if (textures[i].Texture == null)
                    {
                        textures[i].Texture = GD.Load<Texture2D>($"res://ui/inventory/tools and items/{item}.png");
                        if (amount > 1)
                            labels[i].Text = amount.ToString();
                        break;
                    }
                }
            }
        }
        else
        {
            for (int a = 0; a < amount; a++)
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    if (textures[i].Texture == null)
                    {
                        textures[i].Texture = GD.Load<Texture2D>($"res://ui/inventory/tools and items/{item}.png");
                        break;
                    }
                }
            }
        }

        _hotbar.mirror_inventory();
    }

    public void remove(string item, int amount = 1)
    {
        if (stacked_items.ContainsKey(item))
        {
            int number = (int)stacked_items[item];

            if (number > amount)
            {
                stacked_items[item] = number - amount;

                for (int i = 0; i < textures.Count; i++)
                {
                    var texture = textures[i].Texture;
                    if (texture != null && item == texture.ResourcePath.GetFile().GetBaseName())
                    {
                        labels[i].Text = (number - amount).ToString();
                        _hotbar.mirror_inventory();
                        return;
                    }
                }
            }
            else
            {
                stacked_items.Remove(item);
            }
        }

        for (int a = 0; a < amount; a++)
        {
            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i].Texture;
                if (texture != null && item == texture.ResourcePath.GetFile().GetBaseName())
                {
                    textures[i].Texture = null;
                    labels[i].Text = "";
                    break;
                }
            }
        }

        _hotbar.mirror_inventory();
        equippedItem = "None";
    }

    public void _on_button_pressed(int number)
    {
        if (Input.IsActionPressed("shift"))
        {
            if (number <= 30)
            {
                for (int i = 30; i < 40; i++)
                {
                    if (textures[i].Texture == null)
                    {
                        textures[i].Texture = textures[number - 1].Texture;
                        labels[i].Text = labels[number - 1].Text;
                        textures[number - 1].Texture = null;
                        labels[number - 1].Text = "";
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    if (textures[i].Texture == null)
                    {
                        textures[i].Texture = textures[number - 1].Texture;
                        labels[i].Text = labels[number - 1].Text;
                        textures[number - 1].Texture = null;
                        labels[number - 1].Text = "";
                        break;
                    }
                }
            }
        }
        else if (textures[number - 1].Texture != null)
        {
            _savedSlot = number;

            DraggedItem.Texture = textures[number - 1].Texture;
            _draggedItemLabel.Text = labels[number - 1].Text;
            DraggedItem.Visible = true;

            textures[number - 1].Texture = null;
            labels[number - 1].Text = "";

            SetPhysicsProcess(true);
        }
    }
}
