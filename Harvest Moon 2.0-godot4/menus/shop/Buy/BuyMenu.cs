using Godot;
using System.Collections.Generic;

public partial class BuyMenu : Control
{
    private SellMenu _sell = null!;
    private GameSoundManager _soundManager = null!;

    private BaseButton _plus1 = null!;
    private BaseButton _plus5 = null!;
    private BaseButton _max = null!;

    private int _gold;
    private Label _yourGold = null!;
    private Label _amount = null!;
    private Label _total = null!;

    private readonly List<BaseButton> _close = new();
    private readonly List<TextureRect> _item = new();
    private readonly List<Label> _costPerItem = new();
    private readonly List<Label> _cart = new();
    private readonly List<Label> _current = new();
    private readonly List<Label> _after = new();
    private readonly List<Label> _totalCost = new();

    private int _amountValue;
    private int _tableRowsFilled;
    private int _currentRow;
    private readonly List<int> _indexOfItemsChosen = new();

    private TextureRect _chosenItemIndicator = null!;
    private int _indicatorPosition = 1;

    private PlayerInventory _inventory = null!;

    private static readonly Dictionary<string, int> BuyableItems = new()
    {
        { "Sickle", 50 }, { "Axe", 200 }, { "Hammer", 500 },
        { "TurnipSeeds", 5 }, { "StrawberrySeeds", 10 }, { "EggplantSeeds", 20 }
    };

    private readonly List<TextureRect> _shopTextures = new();
    private readonly List<Label> _shopLabels = new();

    private static readonly List<string> BuyableItemKeys = new(BuyableItems.Keys);

    public override void _Ready()
    {
        _sell = GetParent().GetNode<SellMenu>("Sell");
        _soundManager = GetNode<GameSoundManager>("/root/Game/Sound");
        _plus1 = GetNode<BaseButton>("Amount/ButtonContainer/+1");
        _plus5 = GetNode<BaseButton>("Amount/ButtonContainer/+5");
        _max = GetNode<BaseButton>("Amount/ButtonContainer/Max");
        _yourGold = GetNode<Label>("Your Gold");
        _amount = GetNode<Label>("Amount/Amount");
        _total = GetNode<Label>("Total");
        _chosenItemIndicator = GetNode<TextureRect>("Chosen Item Indicator");
        _inventory = GetNode<PlayerInventory>("/root/Game/Farm/Player/UI/Inventory");

        foreach (var button in GetNode("Purchase Grid").GetChildren())
        {
            var children = ((Node)button).GetChildren();
            _shopTextures.Add((TextureRect)children[0]);
            _shopLabels.Add((Label)children[1]);
        }

        int idx = 0;
        foreach (var kvp in BuyableItems)
        {
            if (idx >= _shopTextures.Count) break;
            _shopTextures[idx].Texture = GD.Load<Texture2D>($"res://ui/inventory/tools and items/{kvp.Key}.png");
            _shopLabels[idx].Text = kvp.Value.ToString();
            idx++;
        }

        foreach (var button in GetNode("Purchase Table/Close").GetChildren())
            _close.Add((BaseButton)button);
        foreach (var texture in GetNode("Purchase Table/Item").GetChildren())
            _item.Add((TextureRect)texture);
        foreach (var label in GetNode("Purchase Table/Cost Per Item").GetChildren())
            _costPerItem.Add((Label)label);
        foreach (var label in GetNode("Purchase Table/Cart").GetChildren())
            _cart.Add((Label)label);
        foreach (var label in GetNode("Purchase Table/Current").GetChildren())
            _current.Add((Label)label);
        foreach (var label in GetNode("Purchase Table/After").GetChildren())
            _after.Add((Label)label);
        foreach (var label in GetNode("Purchase Table/Total Cost").GetChildren())
            _totalCost.Add((Label)label);

        update_your_gold();
        _add_purchase_table_row();
    }

    public void reset_menu()
    {
        _clear_purchase_table();
        _move_chosen_item_indicator(1);
        update_your_gold();
    }

    public void update_buy_menu()
    {
        update_your_gold();

        for (int i = 0; i < _tableRowsFilled; i++)
        {
            var itemName = BuyableItemKeys[_indexOfItemsChosen[i] - 1];
            int number = _inventory.get_amount(itemName);
            _current[i].Text = number.ToString();
            _after[i].Text = (int.Parse(_cart[i].Text) + number).ToString();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_sell.Visible) return;

        if (Input.IsActionPressed("ui_up"))
            _move_chosen_item_indicator(_indicatorPosition - 4);
        else if (Input.IsActionPressed("ui_down"))
            _move_chosen_item_indicator(_indicatorPosition + 4);
        else if (Input.IsActionPressed("ui_left"))
            _move_chosen_item_indicator(_indicatorPosition - 1);
        else if (Input.IsActionPressed("ui_right"))
            _move_chosen_item_indicator(_indicatorPosition + 1);
    }

    private void _move_chosen_item_indicator(int position)
    {
        if (position < 1 || position > 16) return;

        _indicatorPosition = position;
        _chosenItemIndicator.Position = new Vector2(
            267 + (((_indicatorPosition - 1) % 4) * 362),
            241 + (((_indicatorPosition - 1) / 4) * 362)
        );

        if (_amountValue == 0 && _tableRowsFilled != 4)
            _delete_purchase_table_row(_currentRow);

        if (_indexOfItemsChosen.Contains(_indicatorPosition))
        {
            _currentRow = _indexOfItemsChosen.IndexOf(_indicatorPosition);
            _amountValue = int.Parse(_cart[_currentRow].Text);
        }
        else
        {
            _add_purchase_table_row();
            _currentRow = _tableRowsFilled;
            _amountValue = 0;
        }

        _update_amount();
        _update_amount_buttons();
    }

    private void _add_purchase_table_row()
    {
        if (_tableRowsFilled == 4) return;

        if (_indicatorPosition <= BuyableItems.Count)
        {
            _item[_tableRowsFilled].Texture = _shopTextures[_indicatorPosition - 1].Texture;
            _costPerItem[_tableRowsFilled].Text = _shopLabels[_indicatorPosition - 1].Text;
            _cart[_tableRowsFilled].Text = "0";

            var itemName = BuyableItemKeys[_indicatorPosition - 1];
            int number = _inventory.get_amount(itemName);
            _current[_tableRowsFilled].Text = number.ToString();
            _after[_tableRowsFilled].Text = number.ToString();
            _totalCost[_tableRowsFilled].Text = "0";
        }
        else
        {
            _delete_purchase_table_row(_tableRowsFilled);
        }
    }

    private void _update_purchase_table_row()
    {
        _cart[_currentRow].Text = _amountValue.ToString();
        int current = int.Parse(_current[_currentRow].Text);
        _after[_currentRow].Text = (_amountValue + current).ToString();
        int costPerItem = int.Parse(_costPerItem[_currentRow].Text);
        _totalCost[_currentRow].Text = (costPerItem * _amountValue).ToString();

        int total = 0;
        for (int i = 0; i < 4; i++)
        {
            if (!string.IsNullOrEmpty(_totalCost[i].Text))
                total += int.Parse(_totalCost[i].Text);
        }
        _total.Text = total.ToString();
    }

    public void _delete_purchase_table_row(int rowIndex)
    {
        int numRowsCurrentlyFilled = 0;
        foreach (var item in _item)
        {
            if (item.Texture != null) numRowsCurrentlyFilled++;
        }

        if (numRowsCurrentlyFilled - rowIndex >= 2)
        {
            int copyOperations = numRowsCurrentlyFilled - rowIndex - 1;
            int ri = rowIndex;
            for (int i = 0; i < copyOperations; i++)
            {
                _item[ri].Texture = _item[ri + 1].Texture;
                _costPerItem[ri].Text = _costPerItem[ri + 1].Text;
                _cart[ri].Text = _cart[ri + 1].Text;
                _current[ri].Text = _current[ri + 1].Text;
                _after[ri].Text = _after[ri + 1].Text;
                _totalCost[ri].Text = _totalCost[ri + 1].Text;
                ri++;
            }
            rowIndex = numRowsCurrentlyFilled - 1;
        }

        _item[rowIndex].Texture = null;
        _costPerItem[rowIndex].Text = "";
        _cart[rowIndex].Text = "";
        _current[rowIndex].Text = "";
        _after[rowIndex].Text = "";
        _totalCost[rowIndex].Text = "";
    }

    private void _clear_purchase_table()
    {
        for (int i = 0; i < 4; i++)
            _delete_purchase_table_row(0);

        _tableRowsFilled = 0;
        _currentRow = 0;
        _indexOfItemsChosen.Clear();
        _total.Text = "";
    }

    public void update_your_gold()
    {
        _yourGold.Text = _inventory.get_amount("Gold").ToString();
        _gold = _inventory.get_amount("Gold");
    }

    private void _update_amount()
    {
        if (_amountValue < 0) _amountValue = 0;
        _amount.Text = _amountValue.ToString();
    }

    private void _update_amount_buttons()
    {
        if (_tableRowsFilled == 4 && !_indexOfItemsChosen.Contains(_indicatorPosition))
        {
            _plus1.Disabled = true;
            _plus5.Disabled = true;
            _max.Disabled = true;
            return;
        }

        int total = string.IsNullOrEmpty(_total.Text) ? 0 : int.Parse(_total.Text);
        int costPerItem = string.IsNullOrEmpty(_costPerItem[_currentRow].Text) ? 0 : int.Parse(_costPerItem[_currentRow].Text);

        if (costPerItem == 0)
        {
            _plus1.Disabled = true;
            _plus5.Disabled = true;
            _max.Disabled = true;
            return;
        }

        _plus5.Disabled = total + (costPerItem * 5) > _gold;
        bool cantBuyOne = total + costPerItem > _gold;
        _plus1.Disabled = cantBuyOne;
        _max.Disabled = cantBuyOne;
    }

    public void _on_Purchase_pressed()
    {
        if (_tableRowsFilled == 0) return;

        _soundManager.play_effect("purchase");

        int total = int.Parse(_total.Text);
        _gold -= total;
        _inventory.remove("Gold", total);

        for (int i = 0; i < _indexOfItemsChosen.Count; i++)
        {
            _inventory.add(BuyableItemKeys[_indexOfItemsChosen[i] - 1], int.Parse(_cart[i].Text));
        }

        reset_menu();
    }

    public void _on_plus_or_minus_amount_pressed(int amountToAdd)
    {
        if (_amountValue == 0 && amountToAdd > 0)
        {
            _tableRowsFilled++;
            _indexOfItemsChosen.Add(_indicatorPosition);
        }

        if (_amountValue != 0 && _amountValue + amountToAdd <= 0)
        {
            _tableRowsFilled--;
            _indexOfItemsChosen.Remove(_indicatorPosition);
        }

        _amountValue += amountToAdd;
        _update_amount();
        _update_purchase_table_row();
        _update_amount_buttons();
    }

    public void _on_Max_pressed()
    {
        if (_amountValue == 0)
        {
            _tableRowsFilled++;
            _indexOfItemsChosen.Add(_indicatorPosition);
        }

        int costPerItem = int.Parse(_costPerItem[_currentRow].Text);
        int totalCost = int.Parse(_totalCost[_currentRow].Text);
        int total = string.IsNullOrEmpty(_total.Text) ? 0 : int.Parse(_total.Text);
        _amountValue = (_gold - total + totalCost) / costPerItem;
        _update_amount();
        _update_purchase_table_row();
        _update_amount_buttons();
    }

    public void _on_Min_pressed()
    {
        if (_amountValue != 0)
        {
            _tableRowsFilled--;
            _indexOfItemsChosen.Remove(_indicatorPosition);
        }

        _amountValue = 0;
        _update_amount();
        _update_purchase_table_row();
        _update_amount_buttons();
    }

    public void _on_Button_pressed(int position)
    {
        _move_chosen_item_indicator(position);
    }
}
