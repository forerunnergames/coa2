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

  public override void _Ready()
  {
    _tileMapLayer = GetNode <TileMapLayer> ("TileMap/Layer0");
    _player = GetNode <Player> ("Player");
  }

  public override void _Input (InputEvent @event)
  {
    if (!Input.IsActionJustReleased ("Click")) return;
    var (mapCoords, terrain) = Tools.GetTileAt (_player.GlobalPosition, _tileMapLayer);
    var (mapCoords2, terrain2) = Tools.GetTileAt (GetLocalMousePosition(), _tileMapLayer);
    Log.Debug ("Player center is at: {mapCoords} ({terrain})", mapCoords, terrain);
    Log.Debug ("Clicked {mapCoords2} ({terrain2})", mapCoords2, terrain2);
    _tileMapLayer.SetCell (mapCoords2);
  }
}
