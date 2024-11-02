using com.forerunnergames.coa.tools;
using Godot;
using NLog;

namespace com.forerunnergames.coa.player;

public partial class Player : CharacterBody2D
{
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  private const float Speed = 300.0f;
  private const float JumpVelocity = -400.0f;
  private readonly float _gravity = ProjectSettings.GetSetting ("physics/2d/default_gravity").AsSingle();
  private RayCast2D _rayLeft = null!;
  private RayCast2D _rayRight = null!;

  public override void _Ready()
  {
    _rayLeft = GetNode <RayCast2D> ("RayCast2D");
    _rayRight = GetNode <RayCast2D> ("RayCast2D");
  }

  public override void _PhysicsProcess (double delta)
  {
    var velocity = Velocity;
    if (!IsOnFloor()) velocity.Y += _gravity * (float)delta;
    if (Input.IsActionJustPressed ("ui_accept") && IsOnFloor()) velocity.Y = JumpVelocity;
    var direction = Input.GetVector ("ui_left", "ui_right", "ui_up", "ui_down");
    velocity.X = direction == Vector2.Zero ? Mathf.MoveToward (Velocity.X, 0, Speed) : direction.X * Speed;
    Velocity = velocity;
    MoveAndSlide();

    var collision = GetLastSlideCollision();

    if (collision?.GetCollider() is TileMapLayer tileMapLayer1)
    {
      var (mapCoords1, terrain1) = Tools.GetTileAt (collision.GetPosition(), tileMapLayer1);
      Log.Debug ("Last slide: {collisionPosition}", collision.GetPosition());
      Log.Debug ("{terrain} {mapCoords}", terrain1, mapCoords1);
      if (terrain1 == "Icy Cliff") Log.Debug ("{terrain} {mapCoords}", terrain1, mapCoords1);
    }

    if (_rayLeft.IsColliding() && _rayLeft.GetCollider() is TileMapLayer tileMapLayer2)
    {
      var (mapCoords, terrain) = Tools.GetTileAt (_rayLeft.GetCollisionPoint(), tileMapLayer2);
      Log.Debug ("Ray left: {rayCollisionPoint}", _rayLeft.GetCollisionPoint());
      Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
      if (terrain != "Icy Cliff") return;
      Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
      Log.Debug ("Player collided with tile: {mapCoords} ({terrain})", mapCoords, terrain);
    }

    if (_rayRight.IsColliding() && _rayRight.GetCollider() is TileMapLayer tileMapLayer3)
    {
      var (mapCoords, terrain) = Tools.GetTileAt (_rayRight.GetCollisionPoint(), tileMapLayer3);
      Log.Debug ("Ray right: {rayCollisionPoint}", _rayRight.GetCollisionPoint());
      Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
      if (terrain != "Icy Cliff") return;
      Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
      Log.Debug ("Player collided with tile: {mapCoords} ({terrain})", mapCoords, terrain);
    }
  }
}
