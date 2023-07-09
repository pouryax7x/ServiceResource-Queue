using Quartz;
using static Quartz.Logging.OperationName;
using System.Collections.Specialized;
using ServiceResource.Dto;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Queue.Entities;
using ServiceResource.Business.Queue;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ServiceResource;

public static class RunQueue
{
    public static async Task<IApplicationBuilder> RunQueues(this IApplicationBuilder app)
    {
        var schedulerFactory = app.ApplicationServices.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();

        var QueueRepository = app.ApplicationServices.GetService<IQueueRepository>();
        var queueReceiverSettings = await QueueRepository.GetQueueSettings();

        foreach (var QSetting in queueReceiverSettings)
        {
            IDictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "MethodName", QSetting.MethodName },
            };

            //Reciver SR
            IJobDetail QueueRecevierSR = JobBuilder.Create<QueueSrSender>()
                .WithIdentity($"QueueRecevierSR_{QSetting.Id}", "QueueRecevierSR")
                .UsingJobData(new JobDataMap(keyValuePairs))
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                .ForJob(QueueRecevierSR)
                .WithIdentity($"QueueRecevierSRTrigger_{QSetting.Id}", "QueueRecevierSR")
                .WithCronSchedule(QSetting.IntervalCronSchedule)
                .Build();

            // schedule job

            await scheduler.ScheduleJob(QueueRecevierSR, trigger1);

            //Reciver CallBack
            if (!string.IsNullOrEmpty(QSetting.CallBackIntervalCronSchedule) &&
                !string.IsNullOrEmpty(QSetting.CallBackAddress) &&
                QSetting.CallBackMaxCallsPerInterval > 0)
            {
                IJobDetail QueueRecevierCallBack = JobBuilder.Create<QueueCallbackSender>()
                   .WithIdentity($"QueueRecevierCallBack_{QSetting.Id}", "QueueRecevierCallBack")
                   .UsingJobData(new JobDataMap(keyValuePairs))
                   .Build();

                ITrigger trigger2 = TriggerBuilder.Create()
                   .ForJob(QueueRecevierCallBack)
                   .WithIdentity($"QueueRecevierCallBackTrigger_{QSetting.Id}", "QueueRecevierCallBack")
                   .WithCronSchedule(QSetting.CallBackIntervalCronSchedule)
                   .Build();

                // schedule job

                await scheduler.ScheduleJob(QueueRecevierCallBack, trigger2);
            }
        }
        await scheduler.Start();
        return app;
    }
}
