using Quartz;
using Quartz.Impl;
using Paymaker;
using System.Collections.Specialized;
using System.Data;
using System.Threading.Tasks;

/// <summary>
/// Summary description for Automation
/// </summary>
public static class Automation {

    public class JobScheduler {

        public async void Start() {
            // construct a scheduler factory
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);

            // get a scheduler
            IScheduler sched = await factory.GetScheduler();
            await sched.Start();
            DBLog.addRecord(DBLogType.EmailAutomation, "Starting scheduler", -1, -1);

            IJobDetail job = JobBuilder.Create<EmailJob>()
                .WithIdentity("EMail trigger", "Email")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithSimpleSchedule
                  (s =>
                     s.WithIntervalInMinutes(1)
                     .RepeatForever()
                   )
                  .StartNow()
                .Build();

            await sched.ScheduleJob(job, trigger);
        }
    }

    public class EmailJob : IJob {

        public async Task Execute(IJobExecutionContext context) {
            try {
                DBLog.addRecord(DBLogType.EmailAutomation, "Checking Timeshet emails", -1, -1);
                G.User.ID = -1;
                G.User.UserID = -1;
                G.User.RoleID = 1;
                TimesheetCycle.checkAutomatedEmails();
            } catch (JobExecutionException e) {
                DBLog.addRecord(DBLogType.EmailAutomation, e.Message, -1, -1);
            }
        }
    }
}