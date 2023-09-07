using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.Sessions;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CRM_mvc.Utilities.Hubs;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task SendMessage(string message, string id, string? images)
    {
        await StoreMessage(message, int.Parse(id), images, Context.User != null);
    }

    public async Task AskForUpdateChat(string id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(Context.User);

            const string message = "لا يوجد محادثة";
            var session = await _context.Sessions.Include(s => s.Chats)
                .ThenInclude(s => s.Message)
                .ThenInclude(s => s.MessageAttachments)
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(s => s.Id == int.Parse(id));
            if (session == null)
            {
                await Clients.Caller.SendAsync("UpdateChat", new
                {
                    Message = message,
                });
            }


            var messages = session?.Chats?.Select(chat => new MessageResponse
            {
                Id = chat.Id,
                Message = chat.Message?.Body,
                FromUser = _context.ChatSessions.FirstOrDefault(v => v.Chat.Id == chat.Id)?.IsUser ?? false,
                DateTime = chat.Message?.UpdatedAt.ToString("hh:mm tt") ?? DateTime.Now.ToString("hh:mm tt"),
                Attachments = Attachments(chat)
            }).ToList();

            session?.Chats?.Where(chat => chat.Message?.ReadDateTime == null).ToList()
                .ForEach(chat =>
                {
                    var msg = chat.Message;
                    if (msg == null) return;
                    msg.ReadDateTime = DateTime.Now;
                    _context.Messages.Update(msg);
                });

            var currentSessions = new List<CurrentSessions>();
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "ADMIN"))
                {
                    var users = _context.Users
                        .Include(u => u.Sessions)
                        .ThenInclude(s => s.Chats)
                        .ThenInclude(s => s.Customer)
                        .ToList();
                    foreach (var u in users)
                    {
                        foreach (var s in u.Sessions)
                        {
                            var currentSession = new CurrentSessions
                            {
                                Id = s.Id,
                                Phone = s.Customer?.PhoneNumber ?? "N/A",
                                Time = s.Chats.MaxBy(chat => chat.UpdatedAt)?.UpdatedAt.ToString("hh:mm tt") ??
                                       DateTime.Now.ToString("hh:mm tt")
                            };
                            if (s.Chats?.LastOrDefault()?.Message?.Body.Length > 35)
                                currentSession.Message = s.Chats?.LastOrDefault()?.Message?.Body[35..] ?? "N/A";
                            else currentSession.Message = s.Chats?.LastOrDefault()?.Message?.Body ?? "N/A";
                            currentSession.Closed = s.UpdatedAt != s.CreatedAt;
                            currentSessions.Add(currentSession);
                        }
                    }


                    currentSessions = currentSessions.DistinctBy(s => s.Id).ToList();
                }
                else
                {
                    user = await _context.Users
                        .Include(u => u.Sessions)
                        .ThenInclude(s => s.Chats)
                        .ThenInclude(s => s.Customer)
                        .FirstOrDefaultAsync(v => v.Id == user.Id);
                    currentSessions = user?.Sessions?.OrderByDescending(v => v.EndDate).Select(s => new CurrentSessions
                    {
                        Id = s.Id,
                        Phone = s.Customer?.PhoneNumber ?? "N/A",
                        Time = s.Chats.MaxBy(chat => chat.UpdatedAt)?.UpdatedAt.ToString("hh:mm tt") ??
                               DateTime.Now.ToString("hh:mm tt"),
                        Message = s.Chats?.Last().Message?.Body ?? "N/A",
                        Closed = s.UpdatedAt != s.CreatedAt
                    }).ToList();
                }
            }

            var model = new ConversationsViewModel
            {
                Id = int.Parse(id),
                Name = session.Customer.Name ?? session.Customer.PhoneNumber,
                Closed = session.StartDate.CompareTo(session.EndDate) != 0,
                Messages = messages,
                CurrentSessions = currentSessions,
            };
            await Clients.All.SendAsync("UpdateChat", model);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("UpdateChat", new
            {
                Message = ex.Message,
            });
        }
    }

    private List<MessageAttachmentResponse>? Attachments(Chat? chat)
    {
        return chat?.Message?.MessageAttachments?.Select(att => new MessageAttachmentResponse
            { Path = att.Path, Type = att.Type })?.ToList();
    }

    private async Task StoreMessage(string message, int id, string? base64, bool isUser)
    {
        // this for another
        var session = await _context.Sessions
            .Include(v => v.Customer)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (session == null)
        {
            const string messageNotify = "الجلسة غير موجودة";
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Message = messageNotify,
                IsUser = isUser,
                Done = true,
            });
        }

        var nowDate = DateTime.Now;
        var msg = new Message
        {
            Title = "رسالة جديدة",
            Body = message,
            SendDateTime = nowDate,
            CreatedAt = nowDate,
            UpdatedAt = nowDate,
        };
        await _context.Messages.AddAsync(msg);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Message = e.Message,
                IsUser = isUser,
                Done = true,
            });
        }

        var chatChannel =
            await _context.ChatChannels.FirstOrDefaultAsync(v => v.Name == ChatChannelEnum.SESSION.ToString());
        if (chatChannel == null)
        {
            chatChannel = new ChatChannel
            {
                Name = ChatChannelEnum.SESSION.ToString(),
                CreatedAt = nowDate,
                UpdatedAt = nowDate,
            };
            await _context.ChatChannels.AddAsync(chatChannel);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", new
                {
                    Message = $"ChatChannel: {e.Message}",
                    IsUser = isUser,
                    Done = true,
                });
            }
        }

        var chat = new Chat
        {
            Message = msg,
            User = session.User,
            ChatChannel = chatChannel,
            Customer = session.Customer,
            CreatedAt = nowDate,
            UpdatedAt = nowDate,
        };
        await _context.Chats.AddAsync(chat);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Message = $"Chat: {e.Message}",
                IsUser = isUser,
                Done = true,
            });
        }

        var chat_sessions = new ChatSessions
        {
            Chat = chat,
            Session = session,
            IsUser = isUser,
            CreatedAt = nowDate,
            UpdatedAt = nowDate,
        };
        await _context.ChatSessions.AddAsync(chat_sessions);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Message = $"ChatSessions: {e.Message}",
                IsUser = isUser,
                Done = true,
            });
        }

        if (base64 != null)
        {
            try
            {
                var loadImage = await LoadImage(base64);
                _context.MessageAttachments.Add(new MessageAttachment
                {
                    Type = AttachmentType.Image,
                    Path = loadImage,
                    Message = msg,
                    CreatedAt = nowDate,
                    UpdatedAt = nowDate
                });
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", new
                {
                    Message = $"All: {e.Message}",
                    IsUser = isUser,
                    Done = true,
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Message = e.Message,
                IsUser = isUser,
                Done = true,
            });
        }

        var user = await _userManager.GetUserAsync(Context.User);
        if (user == null)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Message = message,
                IsUser = isUser,
                Done = true,
            });
        }
        else
        {
            await Clients.User(user.Id).SendAsync("ReceiveMessage", new
            {
                Message = message,
                IsUser = isUser,
                Done = true,
            });
        }
    }

    private async Task<string> LoadImage(string base64)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64);
            var assetsPath = $"{_hostingEnvironment.WebRootPath}\\assets\\images";
            if (!File.Exists(assetsPath))
                Directory.CreateDirectory(assetsPath);

            var fileName = Guid.NewGuid() + ".jpg";
            var fullPath = Path.Combine(assetsPath, fileName);
            var savedPath = Path.Combine("assets", "images", fileName);
            using var ms = new MemoryStream(bytes);
            await using var fs = new FileStream(fullPath, FileMode.Create);
            await ms.CopyToAsync(fs);

            return $"/{savedPath}";
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}