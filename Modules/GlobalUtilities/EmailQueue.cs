using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class EmailQueue
    {
        public static void start_sending(object rvThread)
        {
            RVJob trd = (RVJob)rvThread;

            if (!trd.TenantID.HasValue || !RaaiVanSettings.EmailQueue.EnableEmailQueue(trd.TenantID.Value)) return;

            if (!trd.TenantID.HasValue) return;

            if (!trd.StartTime.HasValue) trd.StartTime = RaaiVanSettings.EmailQueue.StartTime(trd.TenantID.Value);
            if (!trd.EndTime.HasValue) trd.EndTime = RaaiVanSettings.EmailQueue.EndTime(trd.TenantID.Value);
            
            while (true)
            {
                if (!trd.Interval.HasValue) trd.Interval = RaaiVanSettings.EmailQueue.Interval(trd.TenantID.Value);
                else Thread.Sleep(trd.Interval.Value);

                if (!trd.check_time()) continue;

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                sw.Start();

                List<EmailQueueItem> items = GlobalController.get_email_queue_items(trd.TenantID.Value, 
                    RaaiVanSettings.EmailQueue.BatchSize(trd.TenantID.Value));
                List<long> succeedIds = new List<long>();
                foreach (EmailQueueItem itm in items)
                    if(PublicMethods.send_email(trd.TenantID.Value, itm.Email, itm.Title, itm.EmailBody)) succeedIds.Add(itm.ID.Value);
                GlobalController.archive_email_queue_items(trd.TenantID.Value, succeedIds);

                trd.LastActivityDate = DateTime.Now;

                sw.Stop();
                trd.LastActivityDuration = sw.ElapsedMilliseconds;
            }
        }
    }
}
