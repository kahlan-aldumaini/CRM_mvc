using ClosedXML.Excel;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using System;
using System.Linq;
using System.Linq.Dynamic;
using CRM_mvc.Models.Views.CustomerViewModel;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using CRM_mvc.Utilities.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CRM_mvc.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? date = null)
    {
        ViewData["Search"] = search;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        var customers = _context.Customers
            .Where(v => v.DeletedAt == null);
        // search code
        if (!string.IsNullOrEmpty(date))
        {
            List<DateTime> rangeDate = Helper.GetRangeDate(date);
            customers = customers.Where(customer => customer.CreatedAt <= rangeDate.Max() && customer.CreatedAt >= rangeDate.Min());
        }
        if (!string.IsNullOrEmpty(search))
        {
            customers = customers.Where(v => v.Name.Contains(search) || v.PhoneNumber.Contains(search) || v.Email.Contains(search));
        }

        customers = customers
            .Include(v => v.Calls)
            .Include(v => v.Chats);


        var items = await PaginationResponse<Customer>.CreateAsync(customers.AsNoTracking(), page, pageSize);
        ViewData["TotalPage"] = items.TotalPages;

        var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
        var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;
        ViewData["PreviousPage"] = getCustomerUrl(page = previousPage, pageSize, search);
        ViewData["NextPage"] = getCustomerUrl(page = nextPage, pageSize, search);
        ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
        ViewData["EndRowNumber"] = ((page * pageSize) < customers.Count()) ? (page * pageSize) : customers.Count();

        ViewData["TotalItem"] = customers.Count();
        try
        {
            var model = new CustomerViewModel
            {
                Responses = items.ConvertAll((item) =>
                {
                    var chats = _context.Chats
                        .Count(v => item.Chats.Contains(v) &&
                                    v.ChatChannel.Name.Equals(ChatChannelEnum.SESSION.ToString()));

                    return new CustomerResponse
                    {
                        Id = item.Id,
                        Name = item.Name ?? "---",
                        Email = item.Email ?? "---",
                        Phone = item.PhoneNumber,
                        CallsNumber = item.Calls.Count,
                        ChatsNumber = chats,
                    };
                }).ToList()
            };

            return View(model);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }

    private string getCustomerUrl(int page, int pageSize, string search)
    {
        return $"/customer?page={page}&pageSize={pageSize}&search={search}";
    }

    [HttpGet("/customer/export-to-excel")]
    public IActionResult ExportToExcel(string? date = null)
    {
        var customers = _context.Customers
       .Include(customer => customer.Calls)
       .Include(customer => customer.Chats.Where(chat => chat.ChatChannel.Name == ChatChannelEnum.SESSION.ToString())).Where(v => v.DeletedAt == null);
        var fileName = "customers.xlsx";
        if (date != null)
        {
            List<DateTime> rangeDate = Helper.GetRangeDate(date);
            return GenerateExcel(fileName, customers.Where(customer => customer.CreatedAt <= rangeDate.Max() && customer.CreatedAt >= rangeDate.Min()).ToList());
        }
        return GenerateExcel(fileName, customers.ToList());
    }

    private FileResult GenerateExcel(string fileName, IEnumerable<Customer> customers)
    {
        var dataTable = new DataTable("Call");
        dataTable.Columns.AddRange(new DataColumn[]{
                new DataColumn("رقم العميل"),
                new DataColumn("اسم العميل"),
                new DataColumn("ايميل العميل"),
                new DataColumn("عدد الاتصال"),
                new DataColumn("عدد المحادثات"),
            });
        foreach (var item in customers)
        {

            DataRow row = dataTable.NewRow();
            row["رقم العميل"] = item.PhoneNumber;
            row["اسم العميل"] = item.Name ?? "N/A";
            row["ايميل العميل"] = item.Email ?? "N/A";
            row["عدد الاتصال"] = item.Calls.Count;
            row["عدد المحادثات"] = item.Chats.Count;

            dataTable.Rows.Add(row);
        }
        using (XLWorkbook wb = new XLWorkbook())
        {
            wb.Worksheets.Add(dataTable);
            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }


    [HttpGet]
    public async Task<IActionResult> CustomerInformation(int id)
    {
        var customer = await _context.Customers
            .Include(v => v.Chats.Where(v => v.ChatChannel.Name == ChatChannelEnum.SESSION.ToString()))
            .Include(v => v.Calls)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (customer == null)
        {
            HttpContext.Session.SetString(SessionKeys.NotFound, "لا يوجد عميل بهذة البيانات.");
            return RedirectToAction("Index");
        }

        try
        {
            var chats = _context.Answers
                .Where(chat => chat.Calls.Any(call => customer.Calls != null && customer.Calls.Contains(call)))
                .OrderByDescending(v => v.CreatedAt)
                .Include(v => v.Question)
                .Take(3)
                .Select(chat => new LastSession
                {
                    Id = chat.Id,
                    DateTime = chat.Question!.Title,
                    PhoneNumber = chat.UpdatedAt.ToString("MM/dd/yyyy-hh:mm"),
                }).ToList();

            var calls = _context.Calls
                .Where(call => customer.Calls != null && customer.Calls.Contains(call))
                .OrderByDescending(v => v.UpdatedAt)
                .Take(3)
                .Select(call => new LastCall
                {
                    Id = call.Id,
                    DateTime = call.End.ToString("MM/dd/yyyy-hh:mm"),
                    AnsweredBy = call.User.Name,
                    Duration = $"{Helper.getCallDuration(call.Start, call.End)} دقيقة"
                }).ToList();

            var model = new CustomerDetailsResponse
            {
                Id = customer.Id,
                Name = customer.Name ?? "N/A",
                Phone = customer.PhoneNumber,
                Email = customer.Email ?? "N/A",
                Calls = calls,
                Sessions = chats
            };


            //TODO: get customer projects from CRM system

            return View(model);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }
}