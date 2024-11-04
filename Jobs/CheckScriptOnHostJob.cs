﻿using CheckScriptApp.Data;
using CheckScriptApp.Services;
using Microsoft.Extensions.Logging;

namespace CheckScriptApp.Jobs;

public class CheckScriptOnHostJob(
    ILogger<CheckScriptOnHostJob> logger, 
    SettingsConfig settingsConfig, 
    CheckScriptService service)
    : BaseJob(nameof(CheckScriptOnHostJob), logger, null, settingsConfig.IntervalInSeconds)
{
    protected override Task Execute()
    {
        service.CheckAllHostsForScriptScript();

        return Task.CompletedTask;
    }

    protected override void LogError(Exception ex) => logger.LogError("{0}", ex);

    public override string ToString() => nameof(CheckScriptOnHostJob);
}