using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CheckScriptApp.Jobs;

public abstract class BaseJob : IJob
{
    private readonly ILogger<BaseJob> _logger;
    private readonly string? _cronTriggerString;
    private readonly uint? _timeInSeconds;
    private static readonly Dictionary<string, object> LockJob = new();
    
    public IServiceScope? ServiceScope { get; set; }

    protected BaseJob(
        string jobName, 
        ILogger<BaseJob> logger, 
        string? cronTriggerString = null, 
        uint? timeInSeconds = null)
    {
        _logger = logger;
        _cronTriggerString = cronTriggerString;
        _timeInSeconds = timeInSeconds;
        if (!LockJob.TryGetValue(jobName, out _))
        {
            LockJob.Add(jobName, new());
        }
    }

    public Task Execute(IJobExecutionContext context)
    {
        if (!LockJob.TryGetValue(ToString(), out var jobLock))
        {
            return Task.CompletedTask;
        }

        if (!Monitor.TryEnter(jobLock)) return Task.CompletedTask;
        var sw = new Stopwatch();
        try
        {
            sw.Start();
            Execute().Wait();
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
        finally
        {
            Monitor.Exit(jobLock);
        }

        sw.Stop();
        _logger.LogDebug("Job '{0}' закончила работу за {1}ms", ToString(), sw.ElapsedMilliseconds);
        sw.Reset();

        return Task.CompletedTask;
    }

    public TriggerBuilder? ScheduleJobTrigger =>
        !string.IsNullOrEmpty(_cronTriggerString) 
            ? TriggerBuilder.Create()
            .WithCronSchedule(_cronTriggerString, builder => builder.InTimeZone(TimeZoneInfo.Local))
            .StartNow() 
            : _timeInSeconds.HasValue 
                ? TriggerBuilder.Create()
                    .WithSimpleSchedule(sb => sb
                        .WithIntervalInSeconds((int)_timeInSeconds.Value)
                        .RepeatForever())
                    .StartNow()
                : null;

    protected abstract Task Execute();

    protected abstract void LogError(Exception ex);

    public new abstract string ToString();
}