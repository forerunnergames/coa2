using Godot;
using NLog;

namespace com.forerunnergames.coa.tools;

public static class Tools
{
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();

  public static (Vector2I mapCoords, string terrain) GetTileAt (Vector2 localPosition, TileMapLayer tileMapLayer)
  {
    Log.Trace ("GetTileAt local position: {localPosition}", localPosition);
    var cellTemp1 = tileMapLayer.LocalToMap (localPosition); // + new Vector2 (-0.5f, -2));
    var cellTemp2 = (new Vector2 (cellTemp1.X, cellTemp1.Y) / tileMapLayer.Scale).Floor();
    Log.Trace ("cellTemp1 (scaled): {cellTemp1}, cellTemp2: {cellTemp2}", new Vector2 (cellTemp1.X, cellTemp1.Y) / tileMapLayer.Scale, cellTemp2);
    var mapCoords = new Vector2I ((int)cellTemp2.X, (int)cellTemp2.Y);
    var tileData = tileMapLayer.GetCellTileData (mapCoords);

    var terrain = tileData switch
    {
      { TerrainSet: >= 0, Terrain: >= 0 } => tileMapLayer.TileSet.GetTerrainName (tileData.TerrainSet, tileData.Terrain),
      { TerrainSet: < 0 } or { Terrain: < 0 } => "Unrecognized tile type",
      _ => "Empty"
    };

    return (mapCoords, terrain);
  }
}
