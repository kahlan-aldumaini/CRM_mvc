using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM_mvc.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CallApiController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CallApiController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // websocket api to check the new call 
    [HttpGet("/check-new-call", Name = "CheckNewCall")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> CheckNewCall()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return BadRequest(new
            {
                message = "user not found",
                hasCall = false
            });
        }

        var call = _context.Calls
            .Include(v => v.Customer)
            .FirstOrDefault(v => v.User.Id == user.Id && v.End == v.Start && v.DeletedAt == null);
        if (call == null)
            return Ok(new
            {
                message = "No call",
                hasCall = false
            });

        var nextUrl = "/call/" + call.Id;
        if (call.Customer?.Email == null)
        {
            nextUrl = "/call/" + call.Id + "/new";
        }

        return Ok(new
        {
            nextUrl,
            hasCall = true,
            phone = call.Customer?.PhoneNumber,
            name = call.Customer?.Name,
            id = call.Id
        });
    }

    // done
    [HttpGet("/add/{phoneNumber:required}/{extension:int:required}", Name = "AddNewCall")]
    [AllowAnonymous]
    public async Task<IActionResult> AddCall(string phoneNumber, int extension)
    {
        var model = new
        {
            message = "success",
            done = true,
        };
        var customer = _context.Customers.FirstOrDefault(v => v.PhoneNumber == phoneNumber);
        if (customer == null)
        {
            customer = new Customer
            {
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(v =>
            v.Extentions.Any(ext => ext.ExtensionNumber == extension));

        if (user == null)
        {
            return Ok(new
            {
                message = "user not found",
                done = false,
            });
        }

        var hasOtherCall = _context.Calls.Any(
            v => v.Customer != null &&
                 v.Customer.PhoneNumber == phoneNumber &&
                 v.End == v.Start &&
                 v.DeletedAt == null &&
                 v.User.Extentions.Any(e => e.ExtensionNumber == extension)
        );
        if (hasOtherCall)
            return Ok(new
            {
                message = "Duplicate call",
                done = false
            });

        // call type and channel are hard coded
        var callType = _context.CallTypes.FirstOrDefault(v => v.Name == CallTypeEnum.INCOMING.ToString());
        if (callType == null)
        {
            callType = new CallType
            {
                Name = CallTypeEnum.INCOMING.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.CallTypes.Add(callType);
            await _context.SaveChangesAsync();
        }

        var callChannel = _context.CallChannels.FirstOrDefault(v => v.Name == CallChannelEnum.PHONE.ToString());
        if (callChannel == null)
        {
            callChannel = new CallChannel
            {
                Name = CallChannelEnum.PHONE.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.CallChannels.Add(callChannel);
            await _context.SaveChangesAsync();
        }

        var now = DateTime.Now;
        var call = new Call
        {
            Start = now,
            End = now,
            Customer = customer,
            User = user,
            Type = callType,
            Channel = callChannel,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.Calls.Add(call);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    [HttpGet("/add-outgoing/{phone}")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> AddCall(string phone)
    {
        var user = await _userManager.GetUserAsync(User);
        var extensionUser = await _context.ExtensionUsers.FirstOrDefaultAsync(v =>
            v.DeletedAt == null &&
            v.UsersId == user.Id
        );

        var customer = _context.Customers.FirstOrDefault(v => v.PhoneNumber == phone);

        if (extensionUser == null)
        {
            const string message = "ث";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Info);
            return RedirectToAction("CustomerInformation", "Customer", new { id = customer.Id });
        }


        var extension = await _context.Extensions.FindAsync(extensionUser.ExtensionId);

        var hasOtherCall = _context.Calls.Any(
            v => v.Customer != null &&
                 v.Customer.PhoneNumber == phone &&
                 v.End == v.Start &&
                 v.DeletedAt == null &&
                 v.User.Extentions.Any(e => e.ExtensionNumber == extension.ExtensionNumber)
        );
        if (hasOtherCall)
        {

            const string message = "لديك مكالمة أخرى في الانتظار";
            const string title = "معلومات";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Info);


            return RedirectToAction("CustomerInformation", "Customer", new { id = customer.Id });
        }

        // call type and channel are hard coded
        var callType = _context.CallTypes.FirstOrDefault(v => v.Name == CallTypeEnum.OUTGOING.ToString());
        if (callType == null)
        {
            callType = new CallType
            {
                Name = CallTypeEnum.OUTGOING.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.CallTypes.Add(callType);
            await _context.SaveChangesAsync();
        }

        var callChannel = _context.CallChannels.FirstOrDefault(v => v.Name == CallChannelEnum.PHONE.ToString());
        if (callChannel == null)
        {
            callChannel = new CallChannel
            {
                Name = CallChannelEnum.PHONE.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.CallChannels.Add(callChannel);
            await _context.SaveChangesAsync();
        }

        var now = DateTime.Now;
        var call = new Call
        {
            Start = now,
            End = now,
            Customer = customer,
            User = user,
            Type = callType,
            Channel = callChannel,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.Calls.Add(call);
        await _context.SaveChangesAsync();

        var calls = _context.Calls.Include(c => c.Answers).Where(c => c.Customer == customer).ToList();

        var answer = calls.SelectMany(c => c.Answers).ToList();
        if (answer != null)
        {
            var callReturnAction = await
                _context.ReturnActions.FirstOrDefaultAsync(v => v.Title == ReturnActionType.CALL.ToString());
            if (callReturnAction == null)
            {
                callReturnAction = new ReturnAction
                {
                    Title = ReturnActionType.CALL.ToString(),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.ReturnActions.Add(callReturnAction);
                await _context.SaveChangesAsync();
            }

            foreach (var ans in answer)
            {
                var answerReturnAction = _context.AnswerReturnActions
                    .Where(v => !v.IsDone && v.ReturnActionId == callReturnAction.Id)
                    .Where(v => ans.Id == v.AnswerId)
                    .ToList();

                if (answerReturnAction != null)
                {
                    foreach (var action in answerReturnAction)
                    {
                        action.IsDone = true;
                        action.UpdatedAt = DateTime.Now;
                        _context.AnswerReturnActions.Update(action);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        const string message1 = "تمت إضافة المكالمة بنجاح";
        const string title1 = "تمت العملية";
        Helper.SendNotification(context: HttpContext, title: title1, message: message1, type: NotifyType.Success);

        return RedirectToAction("Calling", "call", new { id = call.Id });
    }

    [HttpGet("/end-call")]
    public async Task<IActionResult> EndCall(string url)
    {
        var user = await _userManager.GetUserAsync(User);
        var call = await _context.Calls.FirstOrDefaultAsync(v => v.User == user && v.End == v.Start);
        if (call != null)
        {
            call.End = DateTime.Now;
            call.UpdatedAt = DateTime.Now;
            _context.Calls.Update(call);
        }
        else
        {
            const string message = "لا يوجد مكالمة";
            const string title = "خطأ";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);
        }

        const string message1 = "تم إنهاء المكالمة بنجاح";
        const string title1 = "تمت العملية بنجاح";
        Helper.SendNotification(context: HttpContext, title: title1, message: message1, type: NotifyType.Success);

        await _context.SaveChangesAsync();
        return Redirect(url);
    }
}