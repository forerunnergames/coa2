using com.forerunnergames.coa.player;
using com.forerunnergames.coa.tools;
using Godot;
using NLog;

namespace com.forerunnergames.coa.game;

public partial class Game : Node2D
{
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  private TileMapLayer _tileMapLayer = null!;
  private Player _player = null!;
  private Label _label = null!;

  public override void _Ready()
  {
    _tileMapLayer = GetNode <TileMapLayer> ("TileMapLayer");
    _player = GetNode <Player> ("Player");
    _label = GetNode <Label> ("%Label");
    _label.Text = "";
  }

  public override void _Input (InputEvent @event)
  {
    if (Input.IsActionJustPressed ("toggle_debug_Text")) _label.Visible = !_label.Visible;
    var (mapCoords, terrain) = Tools.GetTileAt (ToLocal (_player.GlobalPosition), _tileMapLayer);
    var (mapCoords2, terrain2) = Tools.GetTileAt (GetLocalMousePosition(), _tileMapLayer);
    _label.Text = $"Mouse in Tile: {mapCoords2} ({terrain2}), Player: {mapCoords} ({terrain}), Local Mouse Coords: {GetLocalMousePosition()}";
    if (!Input.IsActionJustReleased ("click")) return;
    Log.Debug ("Player center is at: {mapCoords} ({terrain})", mapCoords, terrain);
    Log.Debug ("Clicked {mapCoords2} ({terrain2})", mapCoords2, terrain2);
    _tileMapLayer.SetCell (mapCoords2);
    _label.Text = $"Mouse: {mapCoords2} ({terrain2}), Player: {mapCoords} ({terrain})";
  }
}
