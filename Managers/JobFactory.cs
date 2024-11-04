using CheckScriptApp.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using ArgumentNullException = System.ArgumentNullException;

namespace CheckScriptApp.Managers;

public class JobFactory : IJobFactory
{
    private readonly ServiceProvider _serviceCollection;

    public JobFactory(ServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection.BuildServiceProvider();
        using var sc = _serviceCollection.CreateScope();
        _ = sc.ServiceProvider.GetService<TaskManager>();
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var jobType = bundle.JobDetail.JobType;
        if (!typeof(BaseJob).IsAssignableFrom(jobType))
        {
            throw new ApplicationException("Некорретный формат Job!");
        }

        IServiceScope? scope = null;
        try
        {
            scope = _serviceCollection.CreateScope();
            BaseJob? job = scope.ServiceProvider
                .GetServices(typeof(BaseJob))
                .OfType<BaseJob>()
                .FirstOrDefault(j => j.GetType() == jobType);
            if (job is not null)
            {
                return job;
            }

            job = scope.ServiceProvider.GetService(jobType) as BaseJob;
            job!.ServiceScope = scope;

            return job;
        }
        catch
        {
            scope?.Dispose();
            
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        if (job is null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        (job as BaseJob)?.ServiceScope?.Dispose();
    }
}