using NLog;
using NLog.Targets;
using Godot;

namespace com.forerunnergames.coa.utilities.logging;

[Target ("GodotConsole")]
public sealed class GodotConsoleLoggingTarget : TargetWithLayout
{
  protected override void Write (LogEventInfo logEvent)
  {
    var logMessage = Layout.Render (logEvent);
    if (!IsLogLevelEnabled (logEvent.Level)) return;

    switch (logEvent.Level.Name.ToLower())
    {
      case "warn":
      {
        GD.PushWarning (logMessage);
        GD.Print (logMessage);
        break;
      }
      case "error" or "fatal":
      {
        GD.PushError (logMessage);
        GD.PrintErr (logMessage);
        break;
      }
      default:
      {
        GD.Print (logMessage);
        break;
      }
    }
  }

  private static bool IsLogLevelEnabled (LogLevel logLevel)
  {
    var config = LogManager.Configuration;
    if (config == null) return false;
    var minLevel = Logging.GetMinConsoleLogLevel();
    return minLevel != null && logLevel >= minLevel;
  }
}
