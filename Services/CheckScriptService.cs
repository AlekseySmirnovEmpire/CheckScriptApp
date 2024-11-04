using System.Text.RegularExpressions;
using CheckScriptApp.Data;
using Microsoft.Extensions.Logging;

namespace CheckScriptApp.Services;

public class CheckScriptService(SettingsConfig settingsConfig, ILogger<CheckScriptService> logger)
{
    public void CheckAllHostsForScriptScript()
    {
        using var finished = new CountdownEvent(1);
        foreach (var obj in settingsConfig.HostsToCheck.Select((x, i) => new { Item = x, Index = i }))
        {
            finished.AddCount();
            try
            {
                ThreadPool.QueueUserWorkItem(async void (_) => await CheckHost(obj.Item), null);
                settingsConfig.CheckSuccessHostCheck(obj.Item);
            }
            catch (Exception ex)
            {
                logger.LogError("{0}", ex);
                settingsConfig.AddFailedHost(obj.Item);
            }
            finally
            {
                finished.Signal();
            }
        }

        finished.Signal();
        finished.Wait();
    }

    private async Task CheckHost(Uri uri)
    {
        var builder = new UriBuilder(uri)
        {
            Port = -1
        };

        var client = new HttpClient();
        using var response = await client.GetAsync(builder.Uri);
        response.EnsureSuccessStatusCode();

        if (Regex
            .Matches(
                await response.Content.ReadAsStringAsync(),
                @$"<script.*?src=[""\'].*?{settingsConfig.ScriptHost.Replace(".", "\\.")}.*?[""\']>.*?<\/script>")
            .SelectMany(m => m.Groups.Values)
            .Any(g => g.Value.Contains(settingsConfig.ScriptHost)))
        {
            Console.WriteLine("{0} - ok", uri.Host);

            return;
        }

        Console.WriteLine("{0} - fail", uri.Host);
    }
}