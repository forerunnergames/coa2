using Godot;
using NLog;

namespace com.forerunnergames.coa.player;

public partial class Player : Node2D
{
  private static readonly Logger Log = LogManager.GetCurrentClassLogger();
  private PlayerBody _body = null!;
  private PlayerLeftHand _leftHand = null!;
  private PlayerRightHand _rightHand = null!;

  public override void _Ready()
  {
    _body = GetNode <PlayerBody> ("PlayerBody");
    _leftHand = GetNode <PlayerLeftHand> ("PlayerLeftHand");
    _rightHand = GetNode <PlayerRightHand> ("PlayerRightHand");
  }
}
