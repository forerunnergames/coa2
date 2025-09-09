using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using NLog;
using NLog.Config;
using NLog.MessageTemplates;
using NLog.Targets;

namespace com.forerunnergames.coa.utilities.logging;

public static class Logging
{
  public static readonly LogLevel DefaultLogLevel = LogLevel.Info;
  public static bool IsLogLevelFlagSet { get; set; }

  public static LogLevel? GetMinConsoleLogLevel()
  {
    var rule = GetConsoleLogRule (GetConsoleLogTarget ("console")); // "GodotConsole" mirrors "console" level, so we ignore it here.
    return rule == null ? null : LogLevel.AllLevels.FirstOrDefault (rule.IsLoggingEnabledForLevel) ?? LogLevel.Off;
  }

  // "GodotConsole" level will always mirror "console" level, if it exists.
  public static void SetMinConsoleLogLevel (LogLevel level)
  {
    var config = LogManager.Configuration;
    if (config == null) return;
    var targetNames = new List <string> { "console", "GodotConsole" };

    // @formatter:off
    targetNames
      .Select (GetConsoleLogTarget)
      .OfType <Target>()
      .Select (GetConsoleLogRule)
      .OfType <LoggingRule>()
      .ToList()
      .ForEach (x => x.SetLoggingLevels (level, LogLevel.Fatal));
    // @formatter:on

    LogManager.ReconfigExistingLoggers();

    // Use GD.Print here so we can always print the log level regardless of level filtering.
    GD.Print ($"{nameof (Logging)}: Set min console log level to [{level}]");
  }

  public static void SetGlobalThresholdLogLevel (LogLevel level)
  {
    LogManager.GlobalThreshold = level;
    GD.Print ($"{nameof (Logging)}: Set global log threshold to [{level}]");
  }

  public static void RegisterGodotConsoleTarget()
  {
    var config = LogManager.Configuration;
    if (config == null) return;
    LogManager.Setup().SetupExtensions (e => e.RegisterTarget <GodotConsoleLoggingTarget> ("GodotConsole"));
    var godotConsoleTarget = new GodotConsoleLoggingTarget { Layout = "${newline}${longdate} ${logger} ${uppercase:${level}} ${message} ${exception:format=tostring}" };
    config.AddTarget ("GodotConsole", godotConsoleTarget);
    config.AddRuleForAllLevels (godotConsoleTarget);
    LogManager.ReconfigExistingLoggers();
  }

  public static void DisableStringQuoting() => LogManager.Configuration = StringQuotingFormatter.DisableFor (LogManager.Configuration);
  public static LoggingRule? GetConsoleLogRule (Target? target) => LogManager.Configuration.LoggingRules.FirstOrDefault (r => r.Targets.Contains (target));
  private static Target? GetConsoleLogTarget (string name) => LogManager.Configuration.AllTargets.FirstOrDefault (x => x.Name == name);

  // See https://github.com/NLog/NLog/issues/3556
  private class StringQuotingFormatter : IValueFormatter
  {
    private bool IsQuotingStrings { get; set; }
    private readonly IValueFormatter _originalFormatter;
    private StringQuotingFormatter (IValueFormatter originalFormatter) => _originalFormatter = originalFormatter;

    public bool FormatValue (object value, string format, CaptureType captureType, IFormatProvider formatProvider,
      StringBuilder builder)
    {
      if (IsQuotingStrings || captureType != CaptureType.Normal)
      {
        return _originalFormatter.FormatValue (value, format, captureType, formatProvider, builder);
      }

      // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
      switch (Convert.GetTypeCode (value))
      {
        case TypeCode.String:
        {
          builder.Append ((string)value);
          return true;
        }
        case TypeCode.Char:
        {
          builder.Append ((char)value);
          return true;
        }
        case TypeCode.Empty:
        {
          return true;
        }
      }

      return _originalFormatter.FormatValue (value, format, captureType, formatProvider, builder);
    }

    public static LoggingConfiguration DisableFor (LoggingConfiguration configuration)
    {
      var original = configuration.LogFactory.ServiceRepository.GetService (typeof (IValueFormatter)) as IValueFormatter;
      var stringQuoting = new StringQuotingFormatter (original!) { IsQuotingStrings = false };
      configuration.LogFactory.ServiceRepository.RegisterService (typeof (IValueFormatter), stringQuoting);
      return configuration;
    }
  }
}
