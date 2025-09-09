using Godot;

namespace com.forerunnergames.coa.player;

public partial class PlayerBodyAnchor : RigidBody2D
{
  [Export] public CharacterBody2D Character = null!;
  [Export] public PlayerHand LeftHand = null!;
  [Export] public PlayerHand RightHand = null!;
  public bool IsFollowingCharacter = true; // True: Follows CharacterBody2D every frame, otherwise acts as normal dynamic rigid body the joints can pull.
  private PinJoint2D _leftHandPinJoint = null!;
  private PinJoint2D _rightHandPinJoint = null!;

  public override void _Ready()
  {
    _leftHandPinJoint = GetNode <PinJoint2D> ("LeftHandPinJoint");
    _rightHandPinJoint = GetNode <PinJoint2D> ("RightHandPinJoint");
    _leftHandPinJoint.NodeB = LeftHand.GetPath();
    _rightHandPinJoint.NodeB = RightHand.GetPath();
    TopLevel = true; // Never inherit any transforms.
    FreezeMode = FreezeModeEnum.Kinematic;
    SetFollowingCharacter (IsFollowingCharacter);
  }

  public override void _PhysicsProcess (double delta)
  {
    if (!IsFollowingCharacter) return; // Dynamic mode, do nothing.
    // Kinematic mirror of the character (anchor won’t be moved by solver).
    GlobalTransform = Character.GlobalTransform;
    LinearVelocity = Vector2.Zero;
    AngularVelocity = 0.0f;
    MirrorHandsToPinJoints();
  }

  public override void _IntegrateForces (PhysicsDirectBodyState2D state)
  {
    if (!IsFollowingCharacter) return; // Dynamic mode, do nothing.
    // Kinematic mirror of the character (anchor won’t be moved by solver).
    state.Transform = Character.GlobalTransform;
    state.LinearVelocity = Vector2.Zero;
    state.AngularVelocity = 0.0f;
  }

  public void SetFollowingCharacter (bool isFollowingCharacter)
  {
    IsFollowingCharacter = isFollowingCharacter;
    Freeze = IsFollowingCharacter;
    LeftHand.Freeze = IsFollowingCharacter;
    RightHand.Freeze = IsFollowingCharacter;
    if (IsFollowingCharacter) return;
    LeftHand.CanSleep = false;
    LeftHand.Sleeping = false;
    RightHand.CanSleep = false;
    RightHand.Sleeping = false;
  }

  private void MirrorHandsToPinJoints()
  {
    // Pin pivots are the “sockets”.
    LeftHand.GlobalPosition = _leftHandPinJoint.GlobalPosition;
    RightHand.GlobalPosition = _rightHandPinJoint.GlobalPosition;
    // Keep them tame while following.
    LeftHand.LinearVelocity = Vector2.Zero;
    LeftHand.AngularVelocity = 0.0f;
    RightHand.LinearVelocity = Vector2.Zero;
    RightHand.AngularVelocity = 0.0f;
  }
}
