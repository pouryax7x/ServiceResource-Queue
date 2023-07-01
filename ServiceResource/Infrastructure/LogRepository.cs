using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceResource.Business.Queue;
using ServiceResource.Interfaces;
using ServiceResource.Persistence.Log.Context;
using ServiceResource.Persistence.Log.Entities;
using ServiceResource.Persistence.Queue.Context;
using ServiceResource.Persistence.Queue.Entities;

namespace ServiceResource.Infrastructure
{
    public class LogRepository : ILogRepository
    {
        public DbContextOptionsBuilder<LogContext> Builder { get; set; }
        public LogRepository(IConfiguration configuration)
        {

            Builder = new DbContextOptionsBuilder<LogContext>();
            Builder.UseSqlServer(configuration.GetConnectionString("SR_Log_Address"));

        }



        public async Task Log(RequestLog requestLog)
        {
            try
            {
                using (var dbContext = new LogContext(Builder.Options))
                {
                    await dbContext.RequestLog.AddAsync(requestLog);
                    await dbContext.SaveChangesAsync(new CancellationToken());
                }
            }
            catch (Exception)
            {

            }
        }

        public async Task Log(ResponseLog responseLog)
        {
            try
            {
                using (var dbContext = new LogContext(Builder.Options))
                {
                    await dbContext.ResponseLog.AddAsync(responseLog);
                    await dbContext.SaveChangesAsync(new CancellationToken());
                }
            }
            catch (Exception)
            {

            }
        }

        public async Task Log(QueueLog queueLog)
        {
            try
            {
                using (var dbContext = new LogContext(Builder.Options))
                {
                    await dbContext.QueueLog.AddAsync(queueLog);
                    await dbContext.SaveChangesAsync(new CancellationToken());
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
