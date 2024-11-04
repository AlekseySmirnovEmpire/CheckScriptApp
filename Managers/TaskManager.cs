using CheckScriptApp.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CheckScriptApp.Managers;

public class TaskManager
{
    private readonly IScheduler _scheduler;
    private readonly ILogger<TaskManager> _logger;
    private static bool _jobCreate;

    public TaskManager(IScheduler scheduler, ILogger<TaskManager> logger, IEnumerable<BaseJob> jobs)
    {
        _scheduler = scheduler;
        _logger = logger;
        if (_jobCreate)
        {
            return;
        }

        jobs.ToList().ForEach(Add);

        _jobCreate = true;
    }

    private void Add(BaseJob job)
    {
        var jobKey = new JobKey(job.ToString());
        try
        {
            if (_scheduler.CheckExists(jobKey).Result)
            {
                return;
            }

            _scheduler.ScheduleJob(
                JobBuilder.Create(job.GetType())
                    .WithIdentity(jobKey)
                    .Build(),
                job.ScheduleJobTrigger!.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка при добавлении job '{0}': '{1}'", job.ToString(), ex);
        }
    }
}