using Microsoft.EntityFrameworkCore;
using Quartz;
using ServiceResource.Business.Queue;
using ServiceResource.Business.SR;
using ServiceResource.Infrastructure;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Log.Context;
using ServiceResource.Persistence.Queue.Context;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddNewtonsoftJson();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
        });
        services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });


        //Add Db Context
        services.AddDbContext<QueueContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Queue_DB_Address"),
            builder => builder.MigrationsAssembly(typeof(QueueContext).Assembly.FullName));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }, ServiceLifetime.Transient);


        services.AddDbContext<LogContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SR_Log_Address"),
            builder => builder.MigrationsAssembly(typeof(LogContext).Assembly.FullName));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        }, ServiceLifetime.Transient);


        services.AddTransient<ILogContext>(provider => provider.GetRequiredService<LogContext>());

        services.AddTransient<IQueueContext>(provider => provider.GetRequiredService<QueueContext>());
        services.AddTransient<IQueueHandler, QueueHandler>();
        services.AddTransient<ISR_Service, SR_Service>();
        services.AddSingleton<ILogRepository, LogRepository>();
        services.AddSingleton<IQueueRepository, QueueRepository>();
        //services.AddScoped<ILogger, Logger>();


        return services;
    }
}