using Godot;

namespace com.forerunnergames.coa.ui;

// ReSharper disable once InconsistentNaming
public partial class UI : CanvasLayer
{
  private Label _debugLabel = null!;
  public void SetDebugText (string text) => _debugLabel.Text = text;

  public override void _Ready()
  {
    _debugLabel = GetNode <Label> ("%DebugLabel");
    _debugLabel.Hide();
  }

  public override void _Input (InputEvent @event)
  {
    if (!Input.IsActionJustPressed ("toggle_debug_Text")) return;
    _debugLabel.Visible = !_debugLabel.Visible;
  }
}
