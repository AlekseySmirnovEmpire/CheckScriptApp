using CheckScriptApp.Extensions;

namespace CheckScriptApp.Data;

public class SettingsConfig
{
    private static SettingsConfig? _instance;
    private static readonly object PadLock = new();

    public uint IntervalInSeconds { get; private set; }

    public string ScriptHost { get; }

    public bool KeepRunning { get; set; } = true;

    public List<Uri> HostsToCheck { get; } = [];

    public Dictionary<string, uint> HostsFails { get; } = new();

    private SettingsConfig()
    {
        ScriptHost = Environment.GetEnvironmentVariable("ScriptHost") ?? "advmusic.com";
    }

    public static SettingsConfig Instance
    {
        get
        {
            lock (PadLock)
            {
                _instance ??= new SettingsConfig();
            }

            return _instance;
        }
    }

    public void AddInterval(uint interval) => IntervalInSeconds = interval;

    public void AddNewHost(string host)
    {
        if (string.IsNullOrEmpty(host))
        {
            Console.WriteLine("Передан некорректный хост!");
        }

        var uri = host.CheckIfCorrectHostStringAndReturnUri();
        if (uri == null)
        {
            return;
        }

        HostsToCheck.Add(uri);
    }

    public void AddFailedHost(Uri uri)
    {
        lock (PadLock)
        {
            if (HostsFails.TryGetValue(uri.Host, out _))
            {
                HostsFails[uri.Host] += IntervalInSeconds;
            }

            HostsFails.Add(uri.Host, IntervalInSeconds);
        }
    }

    public void CheckSuccessHostCheck(Uri uri)
    {
        lock (PadLock)
        {
            if (!HostsFails.TryGetValue(uri.Host, out var interval))
            {
                return;
            }

            Console.WriteLine("{0} - recovered after {1}", uri.Host, interval);
            HostsFails.Remove(uri.Host);
        }
    }
}