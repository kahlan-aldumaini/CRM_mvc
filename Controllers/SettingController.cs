using System.Data;
using System.Globalization;
using ClosedXML.Excel;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.CustomerViewModel;
using CRM_mvc.Models.Views.User;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using CRM_mvc.Utilities.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_mvc.Controllers;

[Authorize(Roles = Roles.Admin)]
public class SettingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SettingController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? date = null,
        string? from_date = null, string? to_date = null, string? UserType = null)
    {
        ViewData["Search"] = search;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["FromDate"] = from_date;
        ViewData["ToDate"] = to_date;
        ViewData["UserType"] = UserType;
        var users = _userManager.Users;

        if (!string.IsNullOrEmpty(from_date))
        {
            if (!string.IsNullOrEmpty(to_date))
                users = users.Where(call =>
                    call.CreatedAt.Date >=
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                users = users.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(to_date))
        {
            if (!string.IsNullOrEmpty(from_date))
                users = users.Where(call =>
                    call.CreatedAt.Date <=
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                users = users.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(UserType))
        {
            var role = await _roleManager.FindByNameAsync(UserType);
            if (role != null)
                users = users.Where(user => _context.UserRoles.Any(v => v.UserId == user.Id && v.RoleId == role.Id));
        }

        // search code
        if (search != null)
        {
            // search by customer name or phone number or username
            users = users.Where(v => v.Email.Contains(search) && v.Name.Contains(search));
        }

        var items = await PaginationResponse<ApplicationUser>.CreateAsync(users.AsNoTracking(), page, pageSize);
        ViewData["TotalPage"] = items.TotalPages;

        var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
        var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;

        search ??= "";

        ViewData["PreviousPage"] = getSettingUrl(previousPage, pageSize, search);
        ViewData["NextPage"] = getSettingUrl(page = nextPage, pageSize, search);
        ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
        ViewData["EndRowNumber"] =
            ((page * pageSize) < users.Count()) ? (page * pageSize) : users.Count();

        ViewData["TotalItem"] = users.Count();
        var result = new UserViewModel
        {
            Users = items.ConvertAll((item) => new UserResponse
            {
                Id = item.Id,
                Name = item.Name,
                Role = _context.Roles.Find(_context.UserRoles.FirstOrDefault(v => v.UserId == item.Id)?.RoleId)
                    ?.Name ?? "---",
                IsDeleted = item.DeletedAt != null
            }),
            Roles = _roleManager.Roles.ToList().ConvertAll((item) => new RoleResponse
            {
                Id = item.Id,
                Name = item.Name,
            })
        };
        return View(result);
    }

    private static string getSettingUrl(int page, int pageSize, string search)
    {
        return $"/setting?page={page}&pageSize={pageSize}&search={search}";
    }

    [HttpGet("/setting/export-to-excel")]
    public async Task<FileResult> ExportToExcelAsync(string? from_date = null, string? to_date = null,
        string? UserType = null)
    {
        var users = _userManager.Users;

        if (!string.IsNullOrEmpty(from_date))
        {
            if (!string.IsNullOrEmpty(to_date))
                users = users.Where(call =>
                    call.CreatedAt.Date >=
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                users = users.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(to_date))
        {
            if (!string.IsNullOrEmpty(from_date))
                users = users.Where(call =>
                    call.CreatedAt.Date <=
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            else
                users = users.Where(call =>
                    call.CreatedAt.Date ==
                    DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
        }

        if (!string.IsNullOrEmpty(UserType))
        {
            var role = await _roleManager.FindByNameAsync(UserType);
            if (role != null)
                users = users.Where(user => _context.UserRoles.Any(v => v.UserId == user.Id && v.RoleId == role.Id));
        }

        var fileName = "users.xlsx";
        return GenerateExcel(fileName, users.ToList());
    }

    private FileResult GenerateExcel(string fileName, IEnumerable<ApplicationUser> users)
    {
        var dataTable = new DataTable("Call");
        dataTable.Columns.AddRange(new DataColumn[]
        {
            new DataColumn("اسم المستخدم"),
            new DataColumn("البريد الالكتروني"),
            new DataColumn("الصلاحية"),
            new DataColumn("تاريخ الانشاء"),
            new DataColumn("عدد الاتصالات الواردة"),
            new DataColumn("عدد الاتصالات الصادرة"),
            new DataColumn("عدد الجلسات"),
            new DataColumn("عدد الرسائل الايميل"),
            new DataColumn("عدد الرسائل النصية"),
            new DataColumn("عدد الاتصالات"),
            new DataColumn("الرقم التعريفي")
        });

        foreach (var item in users)
        {
            var row = dataTable.NewRow();
            row["اسم المستخدم"] = item.Name;
            row["البريد الالكتروني"] = item.Email;
            row["الصلاحية"] = _context.Roles.Find(_context.UserRoles.FirstOrDefault(v => v.UserId == item.Id)?.RoleId)
                ?.Name ?? "---";
            row["تاريخ الانشاء"] = item.CreatedAt.ToString("MM/dd/yyyy");
            row["عدد الاتصالات الواردة"] = _context.Calls.Count(v =>
                v.User.Id == item.Id && v.Type.Name == CallTypeEnum.INCOMING.ToString());
            row["عدد الاتصالات الصادرة"] = _context.Calls.Count(v =>
                v.User.Id == item.Id && v.Type.Name == CallTypeEnum.OUTGOING.ToString());
            row["عدد الجلسات"] = _context.Sessions.Count(v => v.User.Id == item.Id);
            row["عدد الرسائل الايميل"] = _context.Chats.Count(v =>
                v.User.Id == item.Id && v.ChatChannel.Name == ChatChannelEnum.EMAIL.ToString());
            row["عدد الرسائل النصية"] = _context.Chats.Count(v =>
                v.User.Id == item.Id && v.ChatChannel.Name == ChatChannelEnum.SMS.ToString());
            row["عدد الاتصالات"] = _context.Calls.Count(v => v.User.Id == item.Id);
            if (_context.ExtensionUsers.Any(v => v.UsersId == item.Id && v.DeletedAt == null))
            {
                var exId = _context.ExtensionUsers.FirstOrDefault(v => v.UsersId == item.Id && v.DeletedAt == null);
                if (exId != null)
                {
                    var ext = _context.Extensions.Find(exId.ExtensionId);
                    if (ext != null)
                        row["الرقم التعريفي"] = ext.ExtensionNumber;
                }
                else
                {
                    row["الرقم التعريفي"] = "---";
                }
            }
            else
                row["الرقم التعريفي"] = "---";

            dataTable.Rows.Add(row);
        }

        using var wb = new XLWorkbook();
        wb.Worksheets.Add(dataTable);
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }


    [HttpGet]
    public async Task<IActionResult> UserInformation(string id)
    {
        var user = await _userManager.Users
            .Include(v => v.Extentions)
            .FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null);

        if (user == null)
        {
            const string message = "حدث خطأ أثناء إضافة العميل";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, title: title,
                type: NotifyType.Error);
            return RedirectToAction("Index");
        }

        var chats = _context.Chats
            .Where(chat => chat.User.Id == user.Id)
            .Include(v => v.ChatChannel)
            .Include(v => v.Message)
            .OrderByDescending(v => v.UpdatedAt)
            .Take(4)
            .Select(chat => new LastSession
            {
                Id = chat.Id,
                Title = chat.Message.Title,
                DateTime = chat.UpdatedAt.ToString("MM/dd/yyyy-hh:mm"),
                PhoneNumber = chat.ChatChannel.Name
            }).ToList();

        var calls = _context.Calls
            .Where(call => call.User.Id == user.Id)
            .Include(v => v.Customer)
            .OrderByDescending(v => v.UpdatedAt)
            .Take(3)
            .Select(call => new LastCall
            {
                Id = call.Id,
                DateTime = call.End.ToString("MM/dd/yyyy-hh:mm"),
                AnsweredBy = call.Customer.Name ?? "N/A",
                Duration = $"{Helper.getCallDuration(call.Start, call.End)} دقيقة"
            }).ToList();

        var role = await _roleManager.FindByIdAsync(_context.UserRoles.First(v => v.UserId == user.Id).RoleId);
        var extensionUser =
            await _context.ExtensionUsers.FirstOrDefaultAsync(v => v.UsersId == user.Id && v.DeletedAt == null);
        var extension = await _context.Extensions.FindAsync(extensionUser?.ExtensionId);

        var userData = new NewUser
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Roles = role.Name,
            Extension = extension?.ExtensionNumber ?? 000,
        };

        var model = new CustomerDetailsResponse
        {
            Id = user.Id,
            Name = user.Name ?? "N/A",
            Phone = user.PhoneNumber ?? "N/A",
            Email = user.Email ?? "N/A",
            Calls = calls,
            Sessions = chats,
            User = userData
        };

        return View(model);
    }

    [HttpPost]
    [Route("/setting/storeUser", Name = "storeUser")]
    public async Task<IActionResult> StoreUser(NewUser model)
    {
        if (!ModelState.IsValid)
        {
            const string message = "حدث خطأ أثناء إضافة العميل";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, title: title,
                type: NotifyType.Error);
            if (model.Id != null)
                return RedirectToAction("UserInformation", new { id = model.Id });
            return RedirectToAction("Index");
        }

        //TODO: get roles 
        var roles = await _roleManager.FindByNameAsync(model.Roles);
        if (roles == null)
        {
            roles = new IdentityRole
            {
                Name = model.Roles
            };
            var roleResult = await _roleManager.CreateAsync(roles);
            if (!roleResult.Succeeded)
            {
                const string message = "حدث خطأ أثناء إضافة الصلاحية";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title,
                    type: NotifyType.Error);
                if (model.Id != null)
                    return RedirectToAction("UserInformation", new { id = model.Id });
                return RedirectToAction("Index");
            }
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            if (model.Password != null)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    const string message = "كلمة المرور غير متطابقة";
                    const string title = "خطأ";
                    Helper.SendNotification(context: HttpContext, message: message, title: title,
                        type: NotifyType.Error);
                    if (model.Id != null)
                        return RedirectToAction("UserInformation", new { id = model.Id });
                    return RedirectToAction("Index");
                }
            }

            user = new ApplicationUser
            {
                Email = model.Email,
                Name = model.Name,
                UserName = model.Name,
            };
            var res = await _userManager.CreateAsync(user, model.Password);
            if (!res.Succeeded)
            {
                const string message = "حدث خطأ أثناء إضافة المستخدم";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title,
                    type: NotifyType.Error);
                if (model.Id != null)
                    return RedirectToAction("UserInformation", new { id = model.Id });
                return RedirectToAction("Index");
            }
        }
        else
        {
            if (model.HasPassword != null && model.HasPassword == "on")
            {
                if (model.Password != null)
                {
                    if (model.Password != model.ConfirmPassword)
                    {
                        const string message = "كلمة المرور غير متطابقة";
                        const string title = "خطأ";
                        Helper.SendNotification(context: HttpContext, message: message, title: title,
                            type: NotifyType.Error);
                        if (model.Id != null)
                            return RedirectToAction("UserInformation", new { id = model.Id });
                        return RedirectToAction("Index");
                    }
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resPass = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!resPass.Succeeded)
                {
                    const string message = "حدث خطأ أثناء تعديل كلمة المرور";
                    const string title = "خطأ";
                    Helper.SendNotification(context: HttpContext, message: message, title: title,
                        type: NotifyType.Error);
                    if (model.Id != null)
                        return RedirectToAction("UserInformation", new { id = model.Id });
                    return RedirectToAction("Index");
                }
            }

            user.Email = model.Email;
            user.Name = model.Name;
            user.UserName = model.Name;
            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded)
            {
                const string message = "حدث خطأ أثناء تعديل المستخدم";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, message: message, title: title,
                    type: NotifyType.Error);
                if (model.Id != null)
                    return RedirectToAction("UserInformation", new { id = model.Id });
                return RedirectToAction("Index");
            }
        }

        var userRole = await _userManager.GetRolesAsync(user);
        if (userRole.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, userRole);
        }

        await _userManager.AddToRoleAsync(user, model.Roles);

        if (roles.Name == MainRoles.USER.ToString())
        {
            var extension = _context.Extensions
                .FirstOrDefault(v => v.ExtensionNumber == model.Extension);
            if (extension == null)
            {
                extension = new Extension()
                {
                    ExtensionNumber = model.Extension,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Extensions.Add(extension);
                await _context.SaveChangesAsync();
            }

            if (_context.ExtensionUsers.Any(v =>
                    v.ExtensionId == extension.Id && v.DeletedAt == null && v.UsersId != user.Id))
            {
                var extentionUserId = _context.ExtensionUsers
                    .Where(v => v.ExtensionId == extension.Id && v.DeletedAt == null && v.UsersId != user.Id)
                    .Select(v => v.UsersId).ToList();
                var usage = await _context.Users.FirstOrDefaultAsync(v =>
                    extentionUserId.Contains(v.Id) && v.DeletedAt == null);
                if (usage != null)
                {
                    var message = $"الرقم {model.Extension} مستخدم من قبل {usage.Name}";
                    const string title = "خطأ";
                    Helper.SendNotification(context: HttpContext, message: message, title: title,
                        type: NotifyType.Error);
                    if (model.Id != null)
                        return RedirectToAction("UserInformation", new { id = model.Id });
                    return RedirectToAction("Index");
                }
            }


            var extensionUser =
                _context.ExtensionUsers
                    .Where(v => v.ExtensionId != extension.Id && v.UsersId == user.Id && v.DeletedAt == null)
                    .ToList();
            foreach (var item in extensionUser)
            {
                item.DeletedAt = DateTime.UtcNow;
                _context.ExtensionUsers.Update(item);
            }

            if (_context.ExtensionUsers
                .Any(v => v.ExtensionId == extension.Id && v.UsersId == user.Id))
            {
                var extUser = _context.ExtensionUsers
                    .FirstOrDefault(v => v.ExtensionId == extension.Id && v.UsersId == user.Id);
                extUser.DeletedAt = null;
                _context.ExtensionUsers.Update(extUser);
            }
            else
            {
                var newExtensionUser = new ExtensionUsers()
                {
                    ExtensionId = extension.Id,
                    UsersId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.ExtensionUsers.AddAsync(newExtensionUser);
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            var extensionUser =
                _context.ExtensionUsers
                    .Where(v => v.UsersId == user.Id).ToList();

            foreach (var item in extensionUser)
            {
                item.DeletedAt = DateTime.UtcNow;
                _context.ExtensionUsers.Update(item);
            }

            await _context.SaveChangesAsync();
        }

        const string message1 = "تمت العملية بنجاح";
        const string title1 = "نجاح";
        Helper.SendNotification(context: HttpContext, message: message1, title: title1,
            type: NotifyType.Success);
        if (model.Id != null)
            return RedirectToAction("UserInformation", new { id = model.Id });
        return RedirectToAction("Index");
    }

    [HttpGet("/setting/deleteUser/{id}", Name = "deleteUser")]
    public IActionResult DeleteUser(string id)
    {
        var user = _userManager.Users.FirstOrDefault(v => v.Id == id);
        if (user == null)
        {
            const string message = "حدث خطأ أثناء حذف العميل";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, title: title,
                type: NotifyType.Error);
            return RedirectToAction("Index");
        }

        user.DeletedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        _context.SaveChanges();
        const string message1 = "تمت العملية بنجاح";
        const string title1 = "نجاح";
        Helper.SendNotification(context: HttpContext, message: message1, title: title1,
            type: NotifyType.Success);
        return RedirectToAction("Index");
    }

    [HttpGet("/setting/restoreUser/{id}", Name = "restoreUser")]
    public IActionResult RestoreUser(string id)
    {
        var user = _userManager.Users.FirstOrDefault(v => v.Id == id);
        if (user == null)
        {
            const string message = "حدث خطأ أثناء إستعادة العميل";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, message: message, title: title,
                type: NotifyType.Error);
            return RedirectToAction("Index");
        }

        user.DeletedAt = null;
        _context.Users.Update(user);
        _context.SaveChanges();
        const string message1 = "تمت العملية بنجاح";
        const string title1 = "نجاح";
        Helper.SendNotification(context: HttpContext, message: message1, title: title1,
            type: NotifyType.Success);
        return RedirectToAction("Index");
    }
}