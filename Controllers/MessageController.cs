using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Message;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Enumerations;
using CRM_mvc.Utilities.Response;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_mvc.Utilities.Config;
using CRM_mvc.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Globalization;
using ClosedXML.Excel;

namespace CRM_mvc.Controllers;

[Authorize]
public class MessageController : Controller
{
    #region Declration

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    #endregion

    public MessageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null,
        string? from_date = null, string? to_date = null, string? employee = null)
    {
        ViewData["Search"] = search;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        var messages = _context.Chats
            .Where(v => v.ChatChannel.Name
                .Equals(ChatChannelEnum.SMS.ToString())
            ).Where(v => v.DeletedAt == null);

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userRole = await _userManager.IsInRoleAsync(user, MainRoles.USER.ToString());
            if (userRole)
            {
                messages = messages.Where(v => v.User != null && v.User.Id == user.Id);
            }
        }

        if (!string.IsNullOrEmpty(from_date))
        {
            if (!string.IsNullOrEmpty(to_date))
                messages = messages.Where(call =>
                    call.CreatedAt.Date >=
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                messages = messages.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(to_date))
        {
            if (!string.IsNullOrEmpty(from_date))
                messages = messages.Where(call =>
                    call.CreatedAt.Date <=
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                messages = messages.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(employee))
        {
            messages = messages.Where(v => v.User != null && v.User.Id == employee);
            ViewData["User"] = employee;
        }

        // search code
        if (!string.IsNullOrEmpty(search))
        {
            // search by customer name or phone number or username
            messages = messages.Where(v =>
                v.Message != null && (v.Message.Title.Contains(search) || v.Message.Body.Contains(search)));
        }

        messages = messages
            .Include(v => v.Message)
            .Include(v => v.User)
            .Include(v => v.Customer);

        var items = await PaginationResponse<Chat>.CreateAsync(messages.AsNoTracking(), page, pageSize);
        ViewData["TotalPage"] = items.TotalPages;

        var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
        var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;

        search ??= "";

        ViewData["PreviousPage"] = GetMessageUrl(previousPage, pageSize, search);
        ViewData["NextPage"] = GetMessageUrl(page = nextPage, pageSize, search);
        ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
        ViewData["EndRowNumber"] = ((page * pageSize) < messages.Count()) ? (page * pageSize) : messages.Count();

        ViewData["TotalItem"] = messages.Count();
        var result = new MessageViewModel
        {
            Responses = items.ConvertAll(item => new MessageResponse
            {
                Id = item.Id,
                Message = item.Message?.Body ?? "",
                Recipient = item.Customer.PhoneNumber,
                Date = item.CreatedAt.ToString("MM/dd/yyyy-hh:")
            })
        };

        return View(result);
    }

    private static string GetMessageUrl(int page, int pageSize, string search)
    {
        return $"/message?page={page}&pageSize={pageSize}&search={search}";
    }

    #region ExportSMSMessage

    [HttpGet("/message/export-to-excel")]
    public async Task<FileResult> ExportToExcel(string? from_date = null, string? to_date = null,
        string? employee = null)
    {
        var messages = _context.Chats.Where(chat => chat.ChatChannel.Name == ChatChannelEnum.SMS.ToString())
            .Include(chat => chat.Message)
            .Include(chat => chat.User)
            .Include(chat => chat.Customer)
            .Where(chat => chat.DeletedAt == null);
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userRole = await _userManager.IsInRoleAsync(user, MainRoles.USER.ToString());
            if (userRole)
            {
                messages = messages.Where(v => v.User!.Id == user.Id);
            }
        }

        if (!string.IsNullOrEmpty(from_date))
        {
            if (!string.IsNullOrEmpty(to_date))
                messages = messages.Where(call =>
                    call.CreatedAt.Date >=
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                messages = messages.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(to_date))
        {
            if (!string.IsNullOrEmpty(from_date))
                messages = messages.Where(call =>
                    call.CreatedAt.Date <=
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                messages = messages.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(employee))
        {
            messages = messages.Where(v => v.User!.Id == employee);
            ViewData["User"] = employee;
        }

        const string fileName = "messages.xlsx";
        return GenerateExcel(fileName, messages.ToList());
    }

    private FileResult GenerateExcel(string fileName, IEnumerable<Chat> messages)
    {
        var dataTable = new DataTable("Message");
        dataTable.Columns.AddRange(new[]
        {
            new DataColumn("رقم العميل"),
            new DataColumn("عنوان الرسالة"),
            new DataColumn("الرسالة"),
            new DataColumn("اسم المرسل"),
            new DataColumn("تاريخ الإرسال")
        });
        foreach (var item in messages)
        {
            DataRow row = dataTable.NewRow();
            row["رقم العميل"] = item.Customer.PhoneNumber;
            row["عنوان الرسالة"] = item.Message?.Title;
            row["الرسالة"] = item.Message?.Body;
            row["اسم المرسل"] = item.User?.Name;
            row["تاريخ الإرسال"] = item.CreatedAt.ToString("dd/MM/yyyy-hh:mm tt");

            dataTable.Rows.Add(row);
        }

        using (XLWorkbook wb = new XLWorkbook())
        {
            wb.Worksheets.Add(dataTable);
            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }
    }

    #endregion

    [HttpGet]
    [Authorize(Roles = Roles.User)]
    public IActionResult GroupMessage()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> GroupMessage(List<string> to, string message)
    {
        var notifyMessage = "";
        var title = "";
        if (!ModelState.IsValid)
        {
            notifyMessage = "الرجاء التأكد من البيانات المدخلة";
            title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: notifyMessage, type: NotifyType.Error);
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            notifyMessage = "المستخدم غير موجود";
            title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: notifyMessage, type: NotifyType.Error);
            return View();
        }


        var sms = new SMS();

        foreach (var cust in to)
        {
            try
            {
                var smsRes = await sms.Send(cust, message, "775835700");
                if (smsRes == null)
                {
                    notifyMessage = "حدث خطأ أثناء إرسال الرسالة";
                    title = "خطأ";
                    Helper.SendNotification(context: HttpContext, title: title, message: notifyMessage,
                        type: NotifyType.Error);
                    return View();
                }
            }
            catch (Exception e)
            {
                notifyMessage = e.Message;
                title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: notifyMessage,
                    type: NotifyType.Error);
            }

            var Message = new Message
            {
                Body = message,
                Title = "SMS",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();

            var customer = await
                _context.Customers.FirstOrDefaultAsync(
                    v => v.PhoneNumber.Contains(cust));
            if (customer == null)
            {
                customer = new Customer
                {
                    PhoneNumber = cust,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            var ChatChannel = await _context.ChatChannels.FirstOrDefaultAsync(v =>
                v.Name.Equals(ChatChannelEnum.SMS.ToString()));
            if (ChatChannel == null)
            {
                ChatChannel = new ChatChannel
                {
                    Name = ChatChannelEnum.SMS.ToString(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                _context.ChatChannels.Add(ChatChannel);
                await _context.SaveChangesAsync();
            }

            var chat = new Chat
            {
                Message = Message,
                User = user,
                Customer = customer,
                ChatChannel = ChatChannel,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.Chats.Add(chat);
        }

        await _context.SaveChangesAsync();

        notifyMessage = "تم إرسال الرسالة بنجاح";
        title = "نجاح";
        Helper.SendNotification(context: HttpContext, title: title, message: notifyMessage, type: NotifyType.Success);
        return RedirectToAction("Index", new { Page = 1, PageSize = 10 });
    }

    [HttpPost]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Index(MessageViewModel model)
    {
        string message;
        string title;
        if (!ModelState.IsValid)
        {
            message = "الرجاء التأكد من البيانات المدخلة";
            title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            message = "المستخدم غير موجود";
            title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);

            return View();
        }


        var sms = new SMS();

        try
        {
            var smsRes = await sms.Send(model.SendMessageViewModel.Recipient, model.SendMessageViewModel.Message,
                "775835700");
            if (smsRes == null)
            {
                message = "حدث خطأ أثناء إرسال الرسالة";
                title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);
                return View();
            }
        }
        catch (Exception e)
        {
            message = e.Message;
            title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);
        }

        var Message = new Message
        {
            Body = model.SendMessageViewModel.Message,
            Title = "SMS",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        _context.Messages.Add(Message);
        await _context.SaveChangesAsync();

        var customer = await
            _context.Customers.FirstOrDefaultAsync(v => v.PhoneNumber.Contains(model.SendMessageViewModel.Recipient));
        if (customer == null)
        {
            customer = new Customer
            {
                PhoneNumber = model.SendMessageViewModel.Recipient,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        var chatChannel = await _context.ChatChannels.FirstOrDefaultAsync(v =>
            v.Name.Equals(ChatChannelEnum.SMS.ToString()));
        if (chatChannel == null)
        {
            chatChannel = new ChatChannel
            {
                Name = ChatChannelEnum.SMS.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.ChatChannels.Add(chatChannel);
            await _context.SaveChangesAsync();
        }

        var chat = new Chat
        {
            Message = Message,
            User = user,
            Customer = customer,
            ChatChannel = chatChannel,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        message = "تم إرسال الرسالة بنجاح";
        title = "نجاح";
        Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Success);
        return RedirectToAction("Index", new { Page = 1, PageSize = 10 });
    }
}