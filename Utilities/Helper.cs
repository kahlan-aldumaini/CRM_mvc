using System.Net.Mail;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Globalization;

namespace CRM_mvc.Utilities;

public static class Helper
{
    public static string getCallDuration(DateTime start, DateTime end)
    {
        var sp = end - start;
        return $"{sp.Minutes}:{sp.Seconds}";
    }

    public static async Task sendEmail(
        ApplicationDbContext _context,
        string receiver,
        ApplicationUser user,
        string subject,
        string from,
        string body,
        SmtpClient client
    )
    {
        var chat = new Chat();
        var customer = await _context.Customers.FindAsync(int.Parse(receiver));

        if (customer != null)
            chat.Customer = customer;
        else
            throw new Exception("لا يوجد مستلم بهذا البريد الالكتروني");


        chat.User = user;
        var dateNow = DateTime.Now;
        chat.CreatedAt = dateNow;
        chat.UpdatedAt = dateNow;

        // chat is not saved in database
        var chatChannel =
            await _context.ChatChannels.FirstOrDefaultAsync(v => v.Name == ChatChannelEnum.EMAIL.ToString());
        if (chatChannel == null)
        {
            chatChannel = new ChatChannel()
            {
                Name = ChatChannelEnum.EMAIL.ToString(),
                CreatedAt = dateNow,
                UpdatedAt = dateNow
            };
            _context.ChatChannels.Add(chatChannel);
            await _context.SaveChangesAsync();
        }

        chat.ChatChannel = chatChannel;

        var message = new Message()
        {
            Title = subject,
            Body = body,
            CreatedAt = dateNow,
            UpdatedAt = dateNow
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        chat.Message = message;

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        client.Send(customer.Email, from, subject, body);
    }

    public static string GetReturnActionType(ApplicationDbContext _context, Answer answer)
    {
        var type = _context.ReturnActions
            .Find(_context.AnswerReturnActions.FirstOrDefault(v => v.AnswerId == answer.Id)?.ReturnActionId)?.Title;
        var arType = "اتصال";
        if (type == ReturnActionType.EMAIL.ToString())
            arType = "بريد إلكتروني";
        else if (type == ReturnActionType.SMS.ToString())
            arType = "رسالة نصية";
        else if (type == ReturnActionType.CALL.ToString())
            arType = "اتصال";
        else arType = "لا يوجد";

        return arType;
    }
    public static DateTime EndOfDay(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
    }

    public static DateTime StartOfDay(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
    }

    public static void SendNotification(HttpContext context, string message, NotifyType type, string title)
    {
        context.Session.SetString(SessionKeys.NotifyMessage, message);
        context.Session.SetString(SessionKeys.NotifyType, type.ToString().ToLower());
        context.Session.SetString(SessionKeys.NotifyTitle, title);
    }

    public static void ClearNotification(HttpContext context)
    {
        context.Session.Remove(SessionKeys.NotifyMessage);
        context.Session.Remove(SessionKeys.NotifyType);
        context.Session.Remove(SessionKeys.NotifyTitle);
    }

    public static List<DateTime> GetRangeDate(string date)
    {
        List<DateTime> dateTimes = new List<DateTime>();
        foreach (string item in date.Split(" - "))
        {
            dateTimes.Add(DateTime.ParseExact(item, "MM/dd/yyyy", CultureInfo.InvariantCulture));
        }
        return dateTimes;
    }
}