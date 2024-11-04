using CheckScriptApp.Data;
using CheckScriptApp.Jobs;
using CheckScriptApp.Managers;
using CheckScriptApp.Services;
using CheckScriptApp.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;

Console.WriteLine("Старт приложения!");
DotnetUtils.LoadEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
var services = new ServiceCollection();

#region Настройка логгирования

services
    .AddLogging(config => config.AddConsole())
    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

#endregion

#region Подключение настроек

services.AddSingleton(typeof(SettingsConfig), DotnetUtils.InitArgs());

#endregion

#region Подключение Сервисов

services.AddScoped<TaskManager>();
services.AddScoped<CheckScriptService>();

#endregion

#region Добавляем Cron Jobs

services.AddScoped<BaseJob, CheckScriptOnHostJob>();

var schedulerFactory = new StdSchedulerFactory();
var scheduler = schedulerFactory.GetScheduler().Result;
services.AddSingleton(typeof(IScheduler), scheduler);

scheduler.JobFactory = new JobFactory(services);

#endregion

#region Работа приложения

try
{
    using var sp = services.BuildServiceProvider();
    var initData = sp.GetRequiredService<SettingsConfig>();
    Console.CancelKeyPress += delegate(object? _, ConsoleCancelEventArgs e) {
        e.Cancel = true;
        initData.KeepRunning = false;
    };

    scheduler.Start().Wait();

    while (initData.KeepRunning) {
        // Do your work in here, in small chunks.
        // If you literally just want to wait until Ctrl+C,
        // not doing anything, see the answer using set-reset events.
    }
}
catch (Exception ex)
{
    Console.WriteLine("Приложение завершается с ошибкой: '{0}'", ex);
}

Console.WriteLine("Завершение работы приложения!");

#endregion