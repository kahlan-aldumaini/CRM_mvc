using CRM_mvc.Context;
using CRM_mvc.Models.Views.Scheduling;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_mvc.Controllers
{
    public class SchedulingController : Controller
    {
        private ApplicationDbContext __context;


        public SchedulingController(ApplicationDbContext context)
        {
            __context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var callReturnActions = await __context.ReturnActions
                .FirstOrDefaultAsync(v => v.Title == ReturnActionType.CALL.ToString());
            if (callReturnActions == null)
            {
                
                const string message = "لم يتم تحديد اي عملية للمكالمات";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);

                
                return RedirectToAction("Index", "Call");
            }

            var scheduling = __context.AnswerReturnActions
                .Where(v => v.ReturnActionId == callReturnActions.Id && v.DeletedAt == null);

            var data = new List<SchedulingResponse>();

            foreach (var schedule in scheduling.ToList())
            {
                var answer = await __context.Answers.FindAsync(schedule.AnswerId);
                var callAnswer = await __context.CallAnswers.FirstOrDefaultAsync(v => v.AnswerId == answer.Id);
                if (callAnswer == null) continue;
                var call = await __context.Calls
                    .Include(call => call.Customer)
                    .FirstOrDefaultAsync(call => call.Id == callAnswer.CallId);
                if (call == null) continue;
                var customer = call.Customer;

                var calender = CalendarType.NotDone;
                if (schedule.DateTime > DateTime.Now)
                {
                    calender = CalendarType.Waiting;
                }

                if (schedule.IsDone)
                {
                    calender = CalendarType.Done;
                }

                var date = schedule.DateTime ?? DateTime.Now;
                var end = Helper.EndOfDay(date).ToString("MM/dd/yyyy HH:mm:ss");
                var start = Helper.StartOfDay(date).ToString("MM/dd/yyyy HH:mm:ss");

                var schedulingResponse = new SchedulingResponse
                {
                    Id = schedule.Id,
                    Url = "/customer/CustomerInformation?id=" + customer.Id,
                    Title = customer.PhoneNumber.ToString(),
                    Start = start,
                    End = end,
                    AllDay = false,
                    ExtendedProps = new ExtendedProps
                    {
                        Calendar = calender.ToString()
                    }
                };


                data.Add(schedulingResponse);
            }

            return View(new schedulingViewModel
            {
                Responses = data
            });
        }
    }
}