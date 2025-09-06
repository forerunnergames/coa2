using com.forerunnergames.coa.game;
using com.forerunnergames.coa.settings;
using com.forerunnergames.coa.utilities;
using Godot;
using NLog;

namespace com.forerunnergames.coa.player;

public partial class PlayerLeftHand : RigidBody2D
{
  [Export] public float HorizontalClimbSpeed = 30.0f;
  [Export] public float VerticalClimbAcceleration = 1200.0f;
  [Export] public float VerticalClimbMaxSpeed = 50.0f;
  [Export] public float Acceleration = 2000.0f;
  [Export] public float JumpVelocity = -400.0f;
  public bool IsBodyOnFloor { get; set; }
  public bool IsIceTimerStopped { get; set; }
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  private Game _game = null!;
  private int _iceCollisions;

  public override void _Ready()
  {
    _game = GetNode <Game> ("/root/Game");
  }

  // public override void _PhysicsProcess (double delta)
  // {
  //   var velocity = Velocity;
  //   var inputDirection = Input.GetVector ("ui_left", "ui_right", "ui_up", "ui_down");
  //   var horizontalMovementInput = Mathf.Sign (inputDirection.X) != 0;
  //   var climbingInput = Mathf.Sign (inputDirection.Y) == -1;
  //   var climbing = climbingInput && IsIceTimerStopped;
  //   var traversing = climbing && horizontalMovementInput && !IsBodyOnFloor;
  //   var fallVelocity = IsBodyOnFloor ? 0.0f : Settings.Gravity * (float)delta;
  //   var climbVelocity = climbing ? -VerticalClimbAcceleration * (float)delta : 0.0f;
  //   var horizontalSpeed = inputDirection.X * (climbing || traversing ? HorizontalClimbSpeed : 0.0f);
  //   var horizontalVelocity = Mathf.MoveToward (velocity.X, horizontalSpeed, Acceleration * (float)delta);
  //   velocity.X = horizontalVelocity;
  //   velocity.Y += fallVelocity + climbVelocity;
  //   velocity.Y = climbingInput ? Mathf.Max (velocity.Y, -VerticalClimbMaxSpeed) : velocity.Y;
  //   Velocity = velocity;
  //   _game.SetDebugText ($"Velocity: ({Velocity.X:F1}, {Velocity.Y:F1})");
  //   MoveAndSlide();
  //   HandleKinematicCollisions();
  //   HandleIceCollisions();
  // }
  //
  // private void HandleKinematicCollisions()
  // {
  //   for (var i = 0; i < GetSlideCollisionCount(); ++i) HandleKinematicCollision (GetSlideCollision (i));
  // }
  //
  // private void HandleKinematicCollision (KinematicCollision2D collision)
  // {
  //   if (collision.GetCollider() is not TileMapLayer tileMapLayer) return;
  //   var angleDegrees = Mathf.RadToDeg (collision.GetAngle (Vector2.Up));
  //   var (mapCoords, terrain) = Tools.GetTileAt (collision.GetPosition(), tileMapLayer);
  //   Log.Debug ("Last slide: {collisionPosition}", collision.GetPosition());
  //   Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
  //   Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
  //   _game.SetDebugText ($"Collider: {terrain} {mapCoords}, Angle: {angleDegrees}");
  // }
  //
  // // We handle these separately using ray casts because the player doesn't actually collide with ice tiles (physics layer is masked out).
  // // Only the ray casts collide with the ice tiles. This is a workaround for not being able to add Area2D's to individual tiles.
  // private void HandleIceCollisions()
  // {
  //   // _iceCollisions = _rays.Count (r => Tools.GetTerrain (r) == "Icy Cliff");
  //   // if (_iceCollisions == 0 || !_iceTimer.IsStopped()) return;
  //   // _iceTimer.Start();
  // }
}
