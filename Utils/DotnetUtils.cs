using System.Text.RegularExpressions;
using CheckScriptApp.Data;

namespace CheckScriptApp.Utils;

public static class DotnetUtils
{
    public static SettingsConfig InitArgs()
    {
        var settings = SettingsConfig.Instance;
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; ++i)
        {
            if (args[i] == "-i" && uint.TryParse(args[++i], out var interval))
            {
                settings.AddInterval(interval);

                continue;
            }

            settings.AddNewHost(args[i]);
        }

        if (settings.IntervalInSeconds == uint.MinValue)
        {
            throw new ArgumentException("Не передано значение параметра -i");
        }

        return settings;
    }

    public static void LoadEnv(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        File.ReadAllLines(path)
            .Select(l => new Regex(@"^([\w_]+)[\s]?=[\s""]?([^""]*)[""]?$")
                .Matches(l)
                .Select(m => m.Groups.Cast<Group>().Select(e => e.Value).Skip(1))
                .First()
                .ToList())
            .Where(e => e.Count == 2)
            .ToList()
            .ForEach(e => Environment.SetEnvironmentVariable(e.First(), e.Last()));
    }
}