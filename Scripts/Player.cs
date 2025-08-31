using System.Collections.Generic;
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
  private readonly float _gravity = ProjectSettings.GetSetting ("physics/2d/default_gravity").AsSingle();
  private Label _label = null!;
  private Timer _iceTimer = null!;
  private readonly List <RayCast2D> _rays = [];
  private int _iceCollisions;

  public override void _Ready()
  {
    _label = GetNode <Label> ("Label");
    _iceTimer = GetNode <Timer> ("IceTimer");
    for (var i = 1; i <= 12; ++i) _rays.Add (GetNode <RayCast2D> ("RayCast2D" + i));
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
    var fallVelocity = isOnFloor ? 0.0f : _gravity * (float)delta;
    var climbVelocity = climbing ? -VerticalClimbAcceleration * (float)delta : 0.0f;
    var horizontalSpeed = inputDirection.X * (climbing || traversing ? HorizontalClimbSpeed : speedBoostInput ? RunSpeed : WalkSpeed);
    var horizontalVelocity = Mathf.MoveToward (velocity.X, horizontalSpeed, Acceleration * (float)delta);
    velocity.X = horizontalVelocity;
    velocity.Y += fallVelocity + climbVelocity;
    velocity.Y = climbingInput ? Mathf.Max (velocity.Y, -VerticalClimbMaxSpeed) : velocity.Y;
    velocity.Y = startJumping ? JumpVelocity : velocity.Y;
    Velocity = velocity;
    _label.Text = "";
    _label.Text += $"V ({Velocity.X:F1}, {Velocity.Y:F1}) ";
    MoveAndSlide();
    for (var i = 0; i < GetSlideCollisionCount(); ++i) HandleCollision (GetSlideCollision (i));

    _iceCollisions = 0;

    foreach (var ray in _rays)
    {
      if (!ray.IsColliding() || ray.GetCollider() is not TileMapLayer tileMapLayer) continue;
      var (mapCoords, terrain) = Tools.GetTileAt (ray.GetCollisionPoint(), tileMapLayer);
      if (terrain != "Icy Cliff") return;
      _iceCollisions++;
    }

    if (_iceCollisions == 0 || !_iceTimer.IsStopped()) return;
    _iceTimer.Start();
  }

  private void HandleCollision (KinematicCollision2D collision)
  {
    if (collision.GetCollider() is not TileMapLayer tileMapLayer1) return;
    var angleDegrees = Mathf.RadToDeg (collision.GetAngle (Vector2.Up));
    var (mapCoords, terrain) = Tools.GetTileAt (collision.GetPosition(), tileMapLayer1);
    Log.Debug ("Last slide: {collisionPosition}", collision.GetPosition());
    Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
    Log.Debug ("{terrain} {mapCoords}", terrain, mapCoords);
    _label.Text += $"Collider: {terrain} {mapCoords}, Angle: {angleDegrees}";
    if (terrain != "Icy Cliff") return;
    var isGroundIce = Mathf.IsZeroApprox (angleDegrees);
    if (isGroundIce) return;
    _iceCollisions++;
  }
}
