using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Email;
using CRM_mvc.Utilities.Config;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using CRM_mvc.Utilities.Response;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM_mvc.Utilities;
using System.Data;
using System.Globalization;
using ClosedXML.Excel;

namespace CRM_mvc.Controllers;

public class EmailController : Controller
{
    #region Declration

    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    #endregion

    public EmailController(SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null,
        string? from_date = null, string? to_date = null,
        string? employee = null)
    {
        ViewData["Search"] = search;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        var mails = _context.Chats
            .Where(v => v.ChatChannel.Name
                .Equals(ChatChannelEnum.EMAIL.ToString())
            ).Where(v => v.DeletedAt == null);


        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            var userRole = await _userManager.IsInRoleAsync(user, MainRoles.USER.ToString());
            if (userRole)
            {
                mails = mails.Where(v => v.User!.Id == user.Id);
            }
        }

        if (!string.IsNullOrEmpty(from_date))
        {
            if (!string.IsNullOrEmpty(to_date))
                mails = mails.Where(call =>
                    call.CreatedAt.Date >=
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                mails = mails.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(to_date))
        {
            if (!string.IsNullOrEmpty(from_date))
                mails = mails.Where(call =>
                    call.CreatedAt.Date <=
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                mails = mails.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(employee))
        {
            mails = mails.Where(v => v.User!.Id == employee);
            ViewData["User"] = employee;
        }

        // search code
        if (!string.IsNullOrEmpty(search))
        {
            // search by customer name or phone number or username
            mails = mails.Where(v => v.Message.Title.Contains(search) || v.Message.Body.Contains(search));
        }

        mails = mails
            .Include(v => v.Message)
            .Include(v => v.User)
            .Include(v => v.Customer);

        var items = await PaginationResponse<Chat>.CreateAsync(mails.AsNoTracking(), page, pageSize);
        ViewData["TotalPage"] = items.TotalPages;

        var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
        var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;
        ViewData["PreviousPage"] = getEmailUrl(page = previousPage, pageSize, search);
        ViewData["NextPage"] = getEmailUrl(page = nextPage, pageSize, search);
        ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
        ViewData["EndRowNumber"] = ((page * pageSize) < mails.Count()) ? (page * pageSize) : mails.Count();

        ViewData["TotalItem"] = mails.Count();
        var result = new EmailViewModel
        {
            Responses = items.ConvertAll((item) => new EmailResponse
            {
                Id = item.Id,
                SenderName = item.User?.Name ?? "",
                Title = item.Message?.Title ?? "",
                Emails = item.Customer.Email ?? "",
                DateTime = item.CreatedAt.ToString("MM/dd/yyyy-hh:mm")
            }).ToList(),
        };

        return View(result);
    }

    private string getEmailUrl(int page, int pageSize, string search)
    {
        return $"/email?page={page}&pageSize={pageSize}&search={search}";
    }

    #region SendEmail

    #region ExportEmail

    [HttpGet("/email/export-to-excel")]
    public async Task<FileResult> ExportToExcel(string? from_date = null, string? to_date = null,
        string? employee = null)
    {
        var emails = _context.Chats.Where(chat => chat.ChatChannel.Name == ChatChannelEnum.EMAIL.ToString())
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
                emails = emails.Where(v => v.User!.Id == user.Id);
            }
        }

        if (!string.IsNullOrEmpty(from_date))
        {
            if (!string.IsNullOrEmpty(to_date))
                emails = emails.Where(call =>
                    call.CreatedAt.Date >=
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                emails = emails.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(to_date))
        {
            if (!string.IsNullOrEmpty(from_date))
                emails = emails.Where(call =>
                    call.CreatedAt.Date <=
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                emails = emails.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(employee))
        {
            emails = emails.Where(v => v.User!.Id == employee);
            ViewData["User"] = employee;
        }

        const string fileName = "email.xlsx";
        return GenerateExcel(fileName, emails.ToList());
    }

    private FileResult GenerateExcel(string fileName, IEnumerable<Chat> emails)
    {
        var dataTable = new DataTable("Email");
        dataTable.Columns.AddRange(new DataColumn[]
        {
            new DataColumn("ايميل العميل"),
            new DataColumn("عنوان الرسالة"),
            new DataColumn("الرسالة"),
            new DataColumn("اسم المرسل"),
            new DataColumn("تاريخ الإرسال")
        });
        foreach (var item in emails)
        {
            DataRow row = dataTable.NewRow();
            row["ايميل العميل"] = item.Customer.Email;
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
    public IActionResult SendEmail(string email)
    {
        if (email != null)
        {
            var customer = _context.Customers.FirstOrDefaultAsync(v => v.Email == email);
            if (customer != null)
            {
                ViewData["email"] = email;
                ViewData["emailId"] = customer.Id;
            }
            else
            {
                return RedirectToAction("SendEmail");
            }
        }

        var model = new SendEmailViewModel
        {
            customers = _context.Customers.Where(v => v.Email != null).ToList()
        };
        return View(model);
    }

    // send mails
    [HttpPost]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> SendEmail(SendEmailViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            const string message = "المستخدم غير موجود";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);

            return RedirectToAction("Index");
        }

        model.NewEmail.From = user.Email;


        var client = Mail.setClint();

        foreach (var receiver in model.NewEmail.To)
        {
            try
            {
                await Helper.sendEmail(_context: _context, receiver: receiver, user: user,
                    subject: model.NewEmail.Subject, body: model.NewEmail.Body, from: model.NewEmail.From,
                    client: client);

                const string message = "تم إرسال البريد الإلكتروني بنجاح";
                const string title = "نجاح";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Success);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        return RedirectToAction("Index");
    }

    [HttpGet("/Email/ReSendEmail/{id:int}")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> ReSendEmail(int id)
    {
        var email = await _context.Chats
            .Where(v => v.ChatChannel.Name.Equals(ChatChannelEnum.EMAIL.ToString()))
            .Include(v => v.Message)
            .Include(v => v.Customer)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (email == null)
        {
            var message = "البريد الإلكتروني غير موجود";
            var title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, type: NotifyType.Error, title: title);
            return RedirectToAction("Index");
        }

        try
        {
            await Helper.sendEmail(_context: _context, receiver: email.Customer.Id.ToString(), user: email.User,
                subject: email.Message.Title, body: email.Message.Body, from: email.User.Email,
                client: Mail.setClint());
            const string message = "تم إعادة إرسال البريد الإلكتروني بنجاح";
            const string title = "نجاح";

            Helper.SendNotification(context: HttpContext, message: message, type: NotifyType.Success, title: title);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }

        return RedirectToAction("Index");
    }

    #endregion

    #region ShowEmail

    [HttpGet("/Email/ShowEmail/{id:int}")]
    public async Task<IActionResult> ShowEmail(int id)
    {
        var email = await _context.Chats
            .Include(v => v.Message)
            .Include(v => v.User)
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (email == null)
        {
            var message = "البريد الإلكتروني غير موجود";
            var title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, type: NotifyType.Error, title: title);
            return RedirectToAction("Index");
        }

        var model = new EmailDetailsViewModel
        {
            Id = email.Message.Id,
            Title = email.Message.Title,
            Body = email.Message.Body,
            SenderName = email.User?.Name ?? "",
            Emails = email.Customer.Email ?? "",
            DateTime = email.CreatedAt.ToString("MM/dd/yyyy-hh:mm")
        };
        return View(model);
    }

    #endregion

    #region Button

    [HttpPost]
    [Route("/Email/button")]
    public async Task<IActionResult> Button()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("SendEmail");
    }

    #endregion
}