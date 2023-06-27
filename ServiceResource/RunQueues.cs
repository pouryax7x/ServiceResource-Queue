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

        List<QueueReceiverSetting> queueReceiverSettings = new List<QueueReceiverSetting>();

        var QueueRepository = app.ApplicationServices.GetService<IQueueRepository>();

        queueReceiverSettings = await QueueRepository.GetReceiverSettingAsync();

        foreach (var QSetting in queueReceiverSettings)
        {
            IDictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "QSetting", QSetting },
            };

            //Reciver SR
            IJobDetail QueueRecevierSR = JobBuilder.Create<QueueSrSender>()
                .WithIdentity($"QueueRecevierSR_{QSetting.Id}", "QueueRecevierSR")
                .UsingJobData(new JobDataMap(keyValuePairs))
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                .ForJob(QueueRecevierSR)
                .WithIdentity($"QueueRecevierSRTrigger_{QSetting.Id}", "QueueRecevierSR")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(QSetting.Interval_Sec).RepeatForever())
                .Build();

            // schedule job

            await scheduler.ScheduleJob(QueueRecevierSR, trigger1);

            //Reciver CallBack
            if (QSetting.CallBackInterval_Sec != 0 &&
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
                   .WithSimpleSchedule(x => x.WithIntervalInSeconds(QSetting.CallBackInterval_Sec).RepeatForever())
                   .Build();

                // schedule job

                await scheduler.ScheduleJob(QueueRecevierCallBack, trigger2);
            }
        }
        await scheduler.Start();
        return app;
    }
}
