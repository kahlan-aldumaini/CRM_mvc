using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Sessions;
using CRM_mvc.Services;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Enumerations;
using CRM_mvc.Utilities.Sessions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using CRM_mvc.Utilities.Config;
using CRM_mvc.Utilities.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CRM_mvc.Controllers
{
    [Authorize]
    public class SessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SessionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
        {
            ViewData["Search"] = search;
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            var user = await _userManager.GetUserAsync(User);
            var chats = _context.Sessions
                .Where(v => v.DeletedAt == null && v.User.Id == user.Id);

            // search code
            if (!string.IsNullOrEmpty(search))
            {
                // search by customer name or phone number or username
                chats = chats.Where(v =>
                    v.Chats.Any(chat => chat.Message.Title.Contains(search) || chat.Message.Body.Contains(search)));
            }

            chats = chats.Include(chat => chat.User).Include(chat => chat.Customer);

            var items = await PaginationResponse<Session>.CreateAsync(chats.AsNoTracking(), page, pageSize);
            ViewData["TotalPage"] = items.TotalPages;

            var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
            var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;
            ViewData["PreviousPage"] = GetSessionUrl(page = previousPage, pageSize, search);
            ViewData["NextPage"] = GetSessionUrl(page = nextPage, pageSize, search);
            ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
            ViewData["EndRowNumber"] = ((page * pageSize) < chats.Count()) ? (page * pageSize) : chats.Count();

            ViewData["TotalItem"] = chats.Count();
            var result = new SessionViewModel
            {
                Conversations = items.ConvertAll(item => new SessionConversation
                {
                    Id = item.Id,
                    UserName = item.User?.Name ?? "User",
                    Extention = 123,
                    Time = item.Chats?.OrderByDescending(chat => chat.UpdatedAt).FirstOrDefault()?.UpdatedAt ??
                           DateTime.Now,
                    Message = item.Chats?.Last().Message?.Body[35..] ?? "N/A"
                }).ToList()
            };

            return View(result);
        }

        private string GetSessionUrl(int page, int pageSize, string search)
        {
            return $"/session?page={page}&pageSize={pageSize}&search={search}";
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AskForSession()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ConfirmSession()
        {
            return View();
        }

        [HttpGet("/Session/Conversation/{id:int}", Name = "Conversation")]
        public IActionResult Conversation(int id)
        {
            ViewData["SessionId"] = id;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AskForSession(AskForSessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // find or create customer 
                var customer = await _context.Customers.FirstOrDefaultAsync(x => x.PhoneNumber == model.Phone);
                if (customer == null)
                {
                    customer = new Customer()
                    {
                        Name = model.Name,
                        PhoneNumber = model.Phone,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Customers.Add(customer);
                }

                var nowDate = DateTime.Now;


                // todo: generation random code with 6 digits and save it in database
                var code = DateTime.Now.Millisecond % 1000000;
                var customerVerification = new CustomerVerification()
                {
                    Code = code.ToString(),
                    Customer = customer,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                _context.CustomerVerifications.Add(customerVerification);

                //todo:send sms to customer with code

                var message = new
                {
                    to = model.Phone,
                    message = $"صكود التحقق هو صالح لمدة 5 دقائق {code}"
                };

                var sms = new SMS();

                try
                {
                    // var smsRes = await sms.Send(message.to, message.message,
                    //     "775835700");
                    // if (smsRes == null)
                    // {
                    //     const string notify = "حدث خطأ أثناء إرسال الرسالة";
                    //     const string title = "خطأ";
                    //     Helper.SendNotification(context: HttpContext, title: title, message: notify,
                    //         type: NotifyType.Error);
                    //     return RedirectToAction("AskForSession");
                    // }
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    var notify = e.Message;
                    const string title = "خطأ";
                    Helper.SendNotification(context: HttpContext, title: notify, message: notify,
                        type: NotifyType.Error);
                    return RedirectToAction("AskForSession");
                }
            }

            return RedirectToAction("ConfirmSession");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmSession(int code)
        {
            var customerVerification = await
                _context.CustomerVerifications.Include(v => v.Customer).FirstOrDefaultAsync(x => x.Code == code.ToString());
            if (customerVerification == null)
            {
                const string notify = "لا يوجد رقم تحقق بهذا الرقم";
                Helper.SendNotification(context: HttpContext,
                                        title: "خطأ",
                                        message: notify,
                                        type: NotifyType.Error);
                return View();
            }
            if (customerVerification.Code != code.ToString())
            {
                const string notify = "كود التحقق غير صحيح";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: notify, message: notify,
                    type: NotifyType.Error);
                return View();
            }
            // if (customerVerification.CreatedAt.AddMinutes(5) < DateTime.Now)
            // {
            //     const string notify = "انتهت صلاحية كود التحقق";
            //     const string title = "خطأ";
            //     Helper.SendNotification(context: HttpContext, title: title, message: notify,
            //         type: NotifyType.Error);
            //     return View();
            // }

            var nowDate = DateTime.Now;
            var availableUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Sessions.Any(session => session.EndDate != null && session.EndDate < nowDate));
            if (availableUser == null)
            {
                const string notify = "لا يوجد مستشار متاح حاليا";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: notify,
                    type: NotifyType.Info);
                return View();
            }
            var session = new Session
            {
                Customer = customerVerification.Customer,
                User = availableUser,
                StartDate = nowDate,
                CreatedAt = nowDate,
                UpdatedAt = nowDate,
            };
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetInt32("SessionId", session.Id);
            return RedirectToAction("CustomerConversation");
        }

        [AllowAnonymous]
        public IActionResult CustomerConversation()
        {
            var sessionId = HttpContext.Session.GetInt32("SessionId");
            if (sessionId == null)
            {
                return RedirectToAction("AskForSession");
            }
            ViewData["SessionId"] = sessionId;
            return View();
        }

        [AllowAnonymous]
        public IActionResult CloseSession()
        {
            var sessionId = HttpContext.Session.GetInt32("SessionId");
            if (sessionId == null)
            {
                return RedirectToAction("AskForSession");
            }
            var session = _context.Sessions.FirstOrDefault(x => x.Id == sessionId);
            if (session == null)
            {
                return RedirectToAction("AskForSession");
            }
            session.UpdatedAt = DateTime.Now;
            session.EndDate = DateTime.Now;
            _context.Sessions.Update(session);
            _context.SaveChanges();
            HttpContext.Session.Remove("SessionId");
            return RedirectToAction("AskForSession");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SessionMessageViewModel model)
        {
            if (ModelState.IsValid)
            {
                string DirectryFIle = "Sessions";
                Message message = new Message()
                {
                };
                foreach (var item in model.Files)
                {
                    message = await this.UploadFile(item, DirectryFIle, message);
                }

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<Message> UploadFile(IFormFile file, string name, Message message)
        {
            // if (file != null)
            // {
            //     string? fileName = await _localFileUploadService.UploadFile(file, name);
            //     //store to database
            //     if (fileName == null)
            //     {
            //         if (message == null)
            //             throw new Exception("حدث خطأ أثناء رفع الملف");
            //         message.MessageAttachments.Add(new MessageAttachment()
            //         {
            //             Path = fileName,
            //             Title = name,
            //             Type = file.GetType().ToString(),
            //             CreatedAt = DateTime.Now,
            //             UpdatedAt = DateTime.Now
            //         });
            //     }
            // }

            return message;
        }
    }
}