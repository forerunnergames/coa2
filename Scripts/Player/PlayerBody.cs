using System.Collections.Generic;
using System.Linq;
using com.forerunnergames.coa.game;
using com.forerunnergames.coa.settings;
using com.forerunnergames.coa.utilities;
using Godot;
using NLog;

namespace com.forerunnergames.coa.player;

public partial class PlayerBody : RigidBody2D
{
  [Export] public PlayerLeftHand LeftHand = null!;
  [Export] public PlayerRightHand RightHand = null!;
  [Export] public float WalkSpeed = 100.0f;
  [Export] public float RunSpeed = 300.0f;
  [Export] public float HorizontalClimbSpeed = 30.0f;
  [Export] public float VerticalClimbAcceleration = 1200.0f;
  [Export] public float VerticalClimbMaxSpeed = 50.0f;
  [Export] public float Acceleration = 2000.0f;
  [Export] public float JumpVelocity = -400.0f;
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  private Game _game = null!;
  private Timer _iceTimer = null!;
  private readonly List <RayCast2D> _rays = [];
  private int _iceCollisions;
  private RayCast2D _groundCheck = null!;

  public override void _Ready()
  {
    _game = GetNode <Game> ("/root/Game");
    _iceTimer = GetNode <Timer> ("IceTimer");
    for (var i = 1; i <= 4; ++i) _rays.Add (GetNode <RayCast2D> ("RayCast2D" + i));

    // Create ground check ray
    _groundCheck = new RayCast2D();
    _groundCheck.TargetPosition = new Vector2(0, 40);
    _groundCheck.CollisionMask = 6; // Same as ground collision
    AddChild(_groundCheck);
  }

  public override void _IntegrateForces(PhysicsDirectBodyState2D state)
  {
    var delta = state.Step;
    var velocity = state.LinearVelocity;
    var inputDirection = Input.GetVector ("ui_left", "ui_right", "ui_up", "ui_down");
    var horizontalMovementInput = Mathf.Sign (inputDirection.X) != 0;
    var jumpInput = Input.IsActionJustPressed ("ui_accept");
    var speedBoostInput = Input.IsActionPressed ("speed_boost");
    var climbingInput = Mathf.Sign (inputDirection.Y) == -1;
    var isIceTimerStopped = _iceTimer.IsStopped();
    var climbing = climbingInput && isIceTimerStopped;
    var isOnFloor = _groundCheck.IsColliding(); // Use raycast for ground detection
    var startJumping = jumpInput && isOnFloor;
    var traversing = climbing && horizontalMovementInput && !isOnFloor;
    var fallVelocity = isOnFloor ? 0.0f : Settings.Gravity * delta;
    var climbVelocity = climbing ? -VerticalClimbAcceleration * delta : 0.0f;
    var horizontalSpeed = inputDirection.X * (climbing || traversing ? HorizontalClimbSpeed : speedBoostInput ? RunSpeed : WalkSpeed);
    var horizontalVelocity = Mathf.MoveToward (velocity.X, horizontalSpeed, Acceleration * (float)delta);
    velocity.X = horizontalVelocity;
    velocity.Y += fallVelocity + climbVelocity;
    velocity.Y = climbingInput ? Mathf.Max (velocity.Y, -VerticalClimbMaxSpeed) : velocity.Y;
    velocity.Y = startJumping ? JumpVelocity : velocity.Y;
    LeftHand.IsBodyOnFloor = isOnFloor;
    RightHand.IsBodyOnFloor = isOnFloor;
    LeftHand.IsIceTimerStopped = isIceTimerStopped;
    RightHand.IsIceTimerStopped = isIceTimerStopped;
    state.LinearVelocity = velocity;
    _game.SetDebugText ($"Velocity: ({velocity.X:F1}, {velocity.Y:F1}), Grounded: {isOnFloor}");
  }

  public override void _PhysicsProcess(double delta)
  {
    HandleIceCollisions();
  }

  private void HandleIceCollisions()
  {
    _iceCollisions = _rays.Count (r => Tools.GetTerrain (r) == "Icy Cliff");
    if (_iceCollisions == 0 || !_iceTimer.IsStopped()) return;
    _iceTimer.Start();
  }
}
