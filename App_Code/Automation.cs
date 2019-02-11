using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
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
                .WithIdentity("Timesheet trigger", "Email")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(9, 00)) // execute job daily at 9:30
                  .StartNow()
                  .Build();

            await sched.ScheduleJob(job, trigger);
        }
    }

    public class EmailJob : IJob {

        public async Task Execute(IJobExecutionContext context) {
            try {
                DBLog.addRecord(DBLogType.EmailAutomation, "Checking Timesheet emails", -1, -1);
                /*   G.User.ID = -1;
                   G.User.UserID = -1;
                   G.User.RoleID = 1;
                   TimesheetCycle.checkAutomatedEmails();*/
            } catch (JobExecutionException e) {
                DBLog.addRecord(DBLogType.EmailAutomation, e.Message, -1, -1);
            }
        }
    }
}