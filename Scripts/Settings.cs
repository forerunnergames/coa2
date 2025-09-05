using Godot;

namespace com.forerunnergames.coa.settings;

public static class Settings
{
  public static readonly float Gravity = ProjectSettings.GetSetting ("physics/2d/default_gravity").AsSingle();
}
