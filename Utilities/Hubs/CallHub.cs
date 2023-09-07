using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Charts;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CRM_mvc.Utilities.Hubs
{
    public class CallHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public CallHub(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task HasCall(string state)
        {
            if (state != HasCallState.NoCall.ToString())
            {
                await Clients.All.SendAsync("ReceiveCall", new { hasCall = false, state = state });
            }

            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null)
            {
                await Clients.Caller.SendAsync("ReceiveCall", new { hasCall = false, state = state });
            }

            var call = _context.Calls
                .Include(v => v.Customer)
                .FirstOrDefault(v => v.User.Id == user.Id && v.End == v.Start && v.DeletedAt == null);
            if (call == null)
            {
                await Clients.Caller.SendAsync("ReceiveCall", new
                {
                    hasCall = false,
                    state = HasCallState.NoCall.ToString()
                });
            }

            var nextUrl = "/call/" + call.Id;
            if (call.Customer?.Email == null)
            {
                nextUrl = "/call/" + call.Id + "/new";
            }

            await Clients.Caller.SendAsync("ReceiveCall", new
            {
                nextUrl,
                hasCall = true,
                phone = call.Customer?.PhoneNumber,
                name = call.Customer?.Name,
                id = call.Id,
                state = HasCallState.HasCall.ToString()
            });
        }

    }
}
