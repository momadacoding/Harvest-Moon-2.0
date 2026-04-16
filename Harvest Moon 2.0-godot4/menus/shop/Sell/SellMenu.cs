using Godot;
using System.Collections.Generic;

public partial class SellMenu : Control
{
    private BuyMenu _buy = null!;
    private GameSoundManager _soundManager = null!;

    private Label _yourGold = null!;
    private BaseButton _plus1 = null!;
    private BaseButton _plus5 = null!;
    private BaseButton _max = null!;

    private int _gold;
    private Label _valuePerItem = null!;
    private Label _youCurrentlyHave = null!;
    private Label _totalValue = null!;
    private Label _amount = null!;

    private int _amountValue;
    private TextureRect _chosenItemIndicator = null!;
    private int _indicatorPosition = 1;

    private PlayerInventory _inventory = null!;

    private static readonly Dictionary<string, int> SellableItems = new()
    {
        { "Sickle", 10 }, { "Axe", 50 }, { "Hammer", 150 },
        { "Turnip", 10 }, { "Strawberry", 20 }, { "Eggplant", 45 },
        { "TurnipSeeds", 1 }, { "StrawberrySeeds", 2 }, { "EggplantSeeds", 5 }
    };

    private readonly Dictionary<string, int> _currentItems = new();
    private static readonly List<string> SellableItemKeys = new(SellableItems.Keys);

    private readonly List<TextureRect> _textures = new();
    private readonly List<Label> _amounts = new();
    private readonly List<Label> _values = new();

    public override void _Ready()
    {
        _buy = GetParent().GetNode<BuyMenu>("Buy");
        _soundManager = GetNode<GameSoundManager>("/root/Game/Sound");
        _yourGold = GetNode<Label>("Your Gold");
        _plus1 = GetNode<BaseButton>("Amount/ButtonContainer/+1");
        _plus5 = GetNode<BaseButton>("Amount/ButtonContainer/+5");
        _max = GetNode<BaseButton>("Amount/ButtonContainer/Max");
        _valuePerItem = GetNode<Label>("Value Per Item");
        _youCurrentlyHave = GetNode<Label>("You Currently Have");
        _totalValue = GetNode<Label>("Total Value");
        _amount = GetNode<Label>("Amount/Amount");
        _chosenItemIndicator = GetNode<TextureRect>("Chosen Item Indicator");
        _inventory = GetNode<PlayerInventory>("/root/Game/Farm/Player/UI/Inventory");

        foreach (var button in GetNode("Item Grid Container").GetChildren())
        {
            var children = ((Node)button).GetChildren();
            _textures.Add((TextureRect)children[0]);
            _amounts.Add((Label)children[1]);
            _values.Add((Label)children[2]);
        }
    }

    public void reset_menu()
    {
        _move_chosen_item_indicator(1);
    }

    public void update_sell_menu()
    {
        for (int i = 0; i < _currentItems.Count; i++)
        {
            _textures[i].Texture = null;
            _amounts[i].Text = "";
            _values[i].Text = "";
        }

        _currentItems.Clear();
        int index = 0;

        foreach (var kvp in SellableItems)
        {
            int amount = _inventory.get_amount(kvp.Key);
            if (amount > 0)
            {
                _textures[index].Texture = GD.Load<Texture2D>($"res://ui/inventory/tools and items/{kvp.Key}.png");
                _amounts[index].Text = amount > 1 ? amount.ToString() : "";
                _values[index].Text = kvp.Value.ToString();
                index++;
                _currentItems[kvp.Key] = amount;
            }
        }

        _update_your_gold();
        _update_amount_buttons();
        _update_value_per_item_and_currently_have();
    }

    public override void _Input(InputEvent @event)
    {
        if (_buy.Visible) return;

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
            300 + (((_indicatorPosition - 1) % 4) * 362),
            334 + (((_indicatorPosition - 1) / 4) * 362)
        );
        _update_value_per_item_and_currently_have();
        _amountValue = 0;
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    private void _update_value_per_item_and_currently_have()
    {
        _valuePerItem.Text = _values[_indicatorPosition - 1].Text;

        if (_indicatorPosition > _currentItems.Count)
        {
            _youCurrentlyHave.Text = "";
            return;
        }

        var font = _youCurrentlyHave.GetThemeFont("font");
        _youCurrentlyHave.AddThemeFontSizeOverride("font_size", 100);

        var currentItemKeys = new List<string>(_currentItems.Keys);
        var itemName = currentItemKeys[_indicatorPosition - 1];
        int number = _inventory.get_amount(itemName);

        var displayName = itemName;
        if (displayName.EndsWith("Seeds"))
        {
            displayName = displayName.Insert(displayName.Length - 5, " ");
            _youCurrentlyHave.AddThemeFontSizeOverride("font_size", 80);
        }

        if (displayName == "Strawberry" && number > 1)
            displayName = "Strawberrie";

        if (number == 1 || itemName.EndsWith("Seeds"))
            _youCurrentlyHave.Text = $"{number} {displayName}";
        else
            _youCurrentlyHave.Text = $"{number} {displayName}s";
    }

    private void _update_amount_and_total_value()
    {
        if (_amountValue < 0) _amountValue = 0;
        _amount.Text = _amountValue.ToString();
        int valuePerItem = string.IsNullOrEmpty(_valuePerItem.Text) ? 0 : int.Parse(_valuePerItem.Text);
        _totalValue.Text = (valuePerItem * _amountValue).ToString();
    }

    private void _update_your_gold()
    {
        _yourGold.Text = _inventory.get_amount("Gold").ToString();
        _gold = _inventory.get_amount("Gold");
    }

    private void _update_amount_buttons()
    {
        int currentAmount = 0;
        var currentItemKeys = new List<string>(_currentItems.Keys);
        var currentItemValues = new List<int>(_currentItems.Values);

        if (_indicatorPosition <= _currentItems.Count)
            currentAmount = currentItemValues[_indicatorPosition - 1];

        _plus5.Disabled = _amountValue + 5 > currentAmount;
        bool cantSellOne = _amountValue + 1 > currentAmount;
        _plus1.Disabled = cantSellOne;
        _max.Disabled = cantSellOne;
    }

    public void _on_Sell_pressed()
    {
        if (_amountValue == 0) return;

        _soundManager.play_effect("sell");

        int totalValue = int.Parse(_totalValue.Text);
        _gold += totalValue;
        _inventory.add("Gold", totalValue);

        var currentItemKeys = new List<string>(_currentItems.Keys);
        _inventory.remove(currentItemKeys[_indicatorPosition - 1], _amountValue);

        _amountValue = 0;
        _update_amount_and_total_value();
        update_sell_menu();
    }

    public void _on_plus_1_pressed()
    {
        _amountValue += 1;
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    public void _on_plus_5_pressed()
    {
        _amountValue += 5;
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    public void _on_Max_pressed()
    {
        var currentItemKeys = new List<string>(_currentItems.Keys);
        _amountValue = _currentItems[currentItemKeys[_indicatorPosition - 1]];
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    public void _on_minus_1_pressed()
    {
        _amountValue -= 1;
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    public void _on_minus_5_pressed()
    {
        _amountValue -= 5;
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    public void _on_Min_pressed()
    {
        _amountValue = 0;
        _update_amount_and_total_value();
        _update_amount_buttons();
    }

    public void _on_Button_pressed(int position)
    {
        _move_chosen_item_indicator(position);
    }
}
