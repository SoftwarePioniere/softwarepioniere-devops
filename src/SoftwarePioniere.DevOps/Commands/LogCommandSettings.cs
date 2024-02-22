using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Serilog.Events;
using Spectre.Console.Cli;

namespace SoftwarePioniere.DevOps.Commands;

public class LogCommandSettings : CommandSettings
{
    [CommandOption("--log-level")]
    [Description("Minimum level for logging")]
    [TypeConverter(typeof(VerbosityConverter))]
#if DEBUG
    [DefaultValue(LogEventLevel.Debug)]
#else
    [DefaultValue(LogEventLevel.Information)]
#endif
    public LogEventLevel LogLevel { get; set; }
}

public sealed class VerbosityConverter : TypeConverter
{
    private readonly Dictionary<string, LogEventLevel> _lookup = new(StringComparer.OrdinalIgnoreCase)
    {
        { "d", LogEventLevel.Debug },
        { "debug", LogEventLevel.Debug },
        // {"v", LogEventLevel.Verbose},
        { "i", LogEventLevel.Information },
        { "info", LogEventLevel.Information },
        { "w", LogEventLevel.Warning },
        { "warning", LogEventLevel.Warning },
        { "e", LogEventLevel.Error },
        { "error", LogEventLevel.Error },
        // {"f", LogEventLevel.Fatal}
    };

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string stringValue)
        {
            var result = _lookup.TryGetValue(stringValue, out var verbosity);
            if (!result)
            {
                const string Format = "The value '{0}' is not a valid verbosity.";
                var message = string.Format(CultureInfo.InvariantCulture, Format, value);
                throw new InvalidOperationException(message);
            }

            return verbosity;
        }

        throw new NotSupportedException("Can't convert value to verbosity.");
    }
}