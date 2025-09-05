using System.Collections.Generic;
using System.Linq;
using com.forerunnergames.coa.game;
using com.forerunnergames.coa.settings;
using com.forerunnergames.coa.tools;
using Godot;
using NLog;

namespace com.forerunnergames.coa.player;

public partial class Player : CharacterBody2D
{
  [Export] public float WalkSpeed = 100.0f;
  [Export] public float RunSpeed = 300.0f;
  [Export] public float HorizontalClimbSpeed = 30.0f;
  [Export] public float VerticalClimbAcceleration = 1200.0f;
  [Export] public float VerticalClimbMaxSpeed = 50.0f;
  [Export] public float Acceleration = 2000.0f;
  [Export] public float JumpVelocity = -400.0f;
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  private Game _game = null!;
  private Timer _iceTimer = null!; // Forces a short fall after slipping on ice, before being allowed to climb again.
  private readonly List <RayCast2D> _rays = [];
  private int _iceCollisions;

  public override void _Ready()
  {
    _game = GetNode <Game> ("/root/Game");
    _iceTimer = GetNode <Timer> ("IceTimer");
    for (var i = 1; i <= 4; ++i) _rays.Add (GetNode <RayCast2D> ("RayCast2D" + i));
  }

  public override void _PhysicsProcess (double delta)
  {
    var velocity = Velocity;
    var inputDirection = Input.GetVector ("ui_left", "ui_right", "ui_up", "ui_down");
    var horizontalMovementInput = Mathf.Sign (inputDirection.X) != 0;
    var jumpInput = Input.IsActionJustPressed ("ui_accept");
    var speedBoostInput = Input.IsActionPressed ("speed_boost");
    var climbingInput = Mathf.Sign (inputDirection.Y) == -1;
    var climbing = climbingInput && _iceTimer.IsStopped();
    var isOnFloor = IsOnFloor();
    var startJumping = jumpInput && isOnFloor;
    var traversing = climbing && horizontalMovementInput && !isOnFloor;
    var fallVelocity = isOnFloor ? 0.0f : Settings.Gravity * (float)delta;
    var climbVelocity = climbing ? -VerticalClimbAcceleration * (float)delta : 0.0f;
    var horizontalSpeed = inputDirection.X * (climbing || traversing ? HorizontalClimbSpeed : speedBoostInput ? RunSpeed : WalkSpeed);
    var horizontalVelocity = Mathf.MoveToward (velocity.X, horizontalSpeed, Acceleration * (float)delta);
    velocity.X = horizontalVelocity;
    velocity.Y += fallVelocity + climbVelocity;
    velocity.Y = climbingInput ? Mathf.Max (velocity.Y, -VerticalClimbMaxSpeed) : velocity.Y;
    velocity.Y = startJumping ? JumpVelocity : velocity.Y;
    Velocity = velocity;
    _game.SetDebugText ($"Velocity: ({Velocity.X:F1}, {Velocity.Y:F1})");
    MoveAndSlide();
    HandleKinematicCollisions();
    HandleIceCollisions();
  }

  private void HandleKinematicCollisions()
  {
    for (var i = 0; i < GetSlideCollisionCount(); ++i) HandleKinematicCollision (GetSlideCollision (i));
  }

  private void HandleKinematicCollision (KinematicCollision2D collision)
  {
    if (collision.GetCollider() is not TileMapLayer tileMapLayer) return;
    var angleDegrees = Mathf.RadToDeg (collision.GetAngle (Vector2.Up));
    var (mapCoords, terrain) = Tools.GetTileAt (collision.GetPosition(), tileMapLayer);
    Log.Debug ("Last slide: {collisionPosition}", collision.GetPosition());
    Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
    Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
    _game.SetDebugText ($"Collider: {terrain} {mapCoords}, Angle: {angleDegrees}");
  }

  // We handle these separately using ray casts because the player doesn't actually collide with ice tiles (physics layer is masked out).
  // Only the ray casts collide with the ice tiles. This is a workaround for not being able to add Area2D's to individual tiles.
  private void HandleIceCollisions()
  {
    _iceCollisions = _rays.Count (r => Tools.GetTerrain (r) == "Icy Cliff");
    if (_iceCollisions == 0 || !_iceTimer.IsStopped()) return;
    _iceTimer.Start();
  }
}
