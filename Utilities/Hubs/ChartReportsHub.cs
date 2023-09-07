using CRM_mvc.Context;
using CRM_mvc.Controllers;
using CRM_mvc.Models.Views.Charts;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.AspNetCore.SignalR;

namespace CRM_mvc.Utilities.Hubs
{
    public class ChartReportsHub : Hub
    {
        private readonly ApplicationDbContext _context;
        
        public ChartReportsHub(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task Refresh(int number)
        {
            var helper =new  HomeController(_context);
            #region weeklyReport

            var calls = _context.Calls;
            var callDay = helper.PerDayMethod(calls);

            var sessions = _context.Sessions;
            var sessionDay = helper.PerDayMethod(sessions);

            var emails = _context.Chats.Where(x => x.ChatChannel.Name == ChatChannelEnum.EMAIL.ToString());
            var emailDay = helper.PerDayMethod(emails);

            var sms = _context.Chats.Where(x => x.ChatChannel.Name == ChatChannelEnum.SMS.ToString());
            var smsDay = helper.PerDayMethod(sms);


            var weeklyReport = new WeeklyReport
            {
                CallDay = callDay,
                Calls = helper.PerDayAverage(callDay),

                SessionDay = sessionDay,
                Sessions = helper.PerDayAverage(sessionDay),

                EmailDay = emailDay,
                Emails = helper.PerDayAverage(emailDay),

                SmsDay = smsDay,
                Sms = helper.PerDayAverage(smsDay)
            };

            #endregion

            #region DailyReport

            var dailyCalls = _context.Calls.Where(x => x.CreatedAt.Date == DateTime.Now.Date);
            var dailyReport = new DailyReport
            {
                IncomingCalls = dailyCalls.Count(x => x.Type.Name == CallTypeEnum.INCOMING.ToString()),
                OutgoingCalls = dailyCalls.Count(x => x.Type.Name == CallTypeEnum.OUTGOING.ToString()),
                Sessions = _context.Sessions.Count(x => x.CreatedAt.Date == DateTime.Now.Date),
                Emails = _context.Chats.Count(x => x.ChatChannel.Name == ChatChannelEnum.EMAIL.ToString() && x.CreatedAt.Date == DateTime.Now.Date),
                Sms = _context.Chats.Count(x => x.ChatChannel.Name == ChatChannelEnum.SMS.ToString() && x.CreatedAt.Date == DateTime.Now.Date),
                CurrentCalls = dailyCalls.Count(x => x.Start == x.End),
                CurrentIncomingCalls = dailyCalls.Count(x => x.Start == x.End && x.Type.Name == CallTypeEnum.INCOMING.ToString()),
                CurrentOutgoingCalls = dailyCalls.Count(x => x.Start == x.End && x.Type.Name == CallTypeEnum.OUTGOING.ToString()),
                IncomingHourlyCalls = helper.HourlyCalls(CallTypeEnum.INCOMING),
                OutgoingHourlyCalls = helper.HourlyCalls(CallTypeEnum.OUTGOING)
            };

            #endregion
            
            var model = new ChartsViewModel
            {
                WeeklyReport = weeklyReport,
                DailyReport = dailyReport
            };

            await Clients.Caller.SendAsync("ReceiveData", model);
        }
    }
}
