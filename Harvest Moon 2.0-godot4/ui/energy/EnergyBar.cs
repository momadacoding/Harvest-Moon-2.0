using Godot;

public partial class EnergyBar : Control
{
    private TextureProgressBar _bar = null!;

    public override void _Ready()
    {
        _bar = GetNode<TextureProgressBar>("Backdrop/Filled Bar");
    }

    public void take_action()
    {
        _bar.Value -= 1;
    }

    public void reset_energy()
    {
        _bar.Value = 50;
    }

    public bool has_energy()
    {
        return _bar.Value != 0;
    }
}
