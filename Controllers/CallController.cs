using System.Data;
using System.Globalization;
using ClosedXML.Excel;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views;
using CRM_mvc.Models.Views.CallViewModels;
using CRM_mvc.Models.Views.CustomerViewModel;
using CRM_mvc.Models.Views.QuestionsAndAnswer;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Constants;
using CRM_mvc.Utilities.Enumerations;
using CRM_mvc.Utilities.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM_mvc.Controllers
{
    [Authorize]
    public class CallController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public CallController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, string? date = null, string? call_type = null, string? from_date = null, string? to_date = null)
        {
            ViewData["Search"] = search;
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["CallType"] = call_type;
            ViewData["FromDate"] = from_date;
            ViewData["ToDate"] = to_date;
            var calls = _context.Calls.Where(v => v.DeletedAt == null);

            // get current user 
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var userRole = await userManager.IsInRoleAsync(user, MainRoles.USER.ToString());
                if (userRole)
                {
                    calls = calls.Where(v => v.User.Id == user.Id);
                }
            }

            if (!string.IsNullOrEmpty(from_date))
            {
                if (!string.IsNullOrEmpty(to_date))
                    calls = calls.Where(call => call.Start.Date >= DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
                else
                    calls = calls.Where(call => call.Start.Date == DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                if (!string.IsNullOrEmpty(from_date))
                    calls = calls.Where(call => call.Start.Date <= DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
                else
                    calls = calls.Where(call => call.Start.Date == DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            }
            if (!string.IsNullOrEmpty(call_type))
            {
                calls = calls.Where(v => v.Type.Name == call_type);
            }
            // search code
            if (!string.IsNullOrEmpty(search))
            {
                // search by customer name or phone number or username
                calls = calls.Where(v =>
                    v.Customer.Name.Contains(search) ||
                    v.Customer.PhoneNumber.Contains(search) ||
                    v.User.Name.Contains(search));
            }

            calls = calls
                .Include(v => v.Customer)
                .Include(v => v.User)
                .Include(v => v.Type)
                .OrderByDescending(v => v.Channel);


            var items = await PaginationResponse<Call>.CreateAsync(calls.AsNoTracking(), page, pageSize);
            ViewData["TotalPage"] = items.TotalPages;

            var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
            var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;
            ViewData["PreviousPage"] = getCallUrl(page = previousPage, pageSize, search);
            ViewData["NextPage"] = getCallUrl(page = nextPage, pageSize, search);
            ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
            ViewData["EndRowNumber"] = ((page * pageSize) < calls.Count()) ? (page * pageSize) : calls.Count();

            ViewData["TotalItem"] = calls.Count();
            var result = new List<CallResponse>();
            foreach (var item in items)
            {
                var call = new CallResponse
                {
                    Id = item.Id,
                    CustomerPhoneNumber = item.Customer?.PhoneNumber ?? "N/A",
                    CustomerName = item.Customer?.Name ?? "N/A",
                    UserName = item.User.Name,
                    TypeName = item.Type.Name,
                    Duration = $"{Helper.getCallDuration(item.Start, item.End)} دقيقة"
                };

                var questions = _context.Questions
                    .Where(v => v.DeletedAt == null)
                    .Include(v => v.Answers)
                    .Count(v => v.Answers.Any(v => v.Calls.Any(call => call == item)));

                call.QuestionCount = questions;
                call.CreatedAt = item.CreatedAt;
                result.Add(call);
            }

            return View(new CallViewModel
            {
                Calls = result,
            });
        }

        private string getCallUrl(int page, int pageSize, string search)
        {
            return $"/call?page={page}&pageSize={pageSize}&search={search}";
        }

        [HttpGet("/call/export-to-excel")]
        public async Task<FileResult> ExportToExcelAsync(string? from_date = null, string? to_date = null, string? call_type = null)
        {
            var calls = _context.Calls
            .Include(call => call.User)
            .Include(call => call.Customer)
            .Include(call => call.Answers)
            .Include(call => call.Type)
            .Where(v => v.DeletedAt == null);

            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var userRole = await userManager.IsInRoleAsync(user, MainRoles.USER.ToString());
                if (userRole)
                {
                    calls = calls.Where(v => v.User.Id == user.Id);
                }
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                if (!string.IsNullOrEmpty(to_date))
                    calls = calls.Where(call => call.Start.Date >= DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
                else
                    calls = calls.Where(call => call.Start.Date == DateTime.ParseExact(from_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                if (!string.IsNullOrEmpty(from_date))
                    calls = calls.Where(call => call.Start.Date <= DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
                else
                    calls = calls.Where(call => call.Start.Date == DateTime.ParseExact(to_date, "MM/dd/yyyy", CultureInfo.InvariantCulture).Date);
            }
            if (!string.IsNullOrEmpty(call_type))
            {
                calls = calls.Where(v => v.Type.Name == call_type);
            }
            var fileName = "calls.xlsx";
            return GenerateExcel(fileName, calls.ToList());
        }

        private FileResult GenerateExcel(string fileName, IEnumerable<Call> calls)
        {
            var dataTable = new DataTable("Call");
            dataTable.Columns.AddRange(new DataColumn[]{
                // رقم العميل 	اسم العميل 	اسم الموظف 	نوع الاتصال 	عدد الاسئلة 	المدة 	تاريخ الاتصال
                new DataColumn("رقم العميل"),
                new DataColumn("اسم العميل"),
                new DataColumn("اسم الموظف"),
                new DataColumn("نوع الاتصال"),
                new DataColumn("عدد الاسئلة"),
                new DataColumn("المدة"),
                new DataColumn("تاريخ الاتصال")
            });
            foreach (var item in calls)
            {
                var questions = _context.Questions
                    .Where(v => v.DeletedAt == null)
                    .Include(v => v.Answers)
                    .Count(v => v.Answers.Any(v => v.Calls.Any(call => call == item)));

                DataRow row = dataTable.NewRow();
                row["رقم العميل"] = item.Customer.PhoneNumber;
                row["اسم العميل"] = item.Customer.Name ?? "N/A";
                row["اسم الموظف"] = item.User.Name;
                row["نوع الاتصال"] = item.Type.Name;
                row["عدد الاسئلة"] = questions.ToString();
                row["المدة"] = $"{Helper.getCallDuration(item.Start, item.End)} دقيقة";
                row["تاريخ الاتصال"] = item.Start.ToString("dd/MM/yyyy");

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

        [HttpGet("/call/{id:int}")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Calling(int id)
        {
            try
            {
                var call = await _context.Calls
                    .Include(v => v.Customer)
                    .Include(v => v.Answers)
                    .FirstOrDefaultAsync(v => v.Id == id);


                // todo: add the project api
                var model = new CallerDetailsViewModel
                {
                    Id = call.Id,
                    Name = call.Customer.Name,
                    Phone = call.Customer.PhoneNumber,
                    Email = call.Customer.Email,
                    Calls = _context.Calls
                        .Where(v => v.Customer != null && v.Customer.PhoneNumber == call.Customer.PhoneNumber)
                        .OrderByDescending(v => v.CreatedAt).Take(3)
                        .Select(v => new LastCall
                        {
                            Id = v.Id,
                            DateTime = v.Start.ToString("MM/dd/yyyy HH:mm"),
                            AnsweredBy = v.User.Name,
                            Duration = $"{v.Start.Minute - v.End.Minute} دقیقه",
                        }).ToList(),
                    Questions = call.Answers
                    .OrderByDescending(v => v.UpdatedAt)
                    .Take(3).Select((CallAnswer) =>
                    {
                        var question =
                            _context.Questions.FirstOrDefault(v => v.Answers.Any(answer => answer.Id == CallAnswer.Id));
                        if (question == null)
                            return new LastQuestion();

                        return new LastQuestion
                        {
                            Id = question.Id,
                            Question = question.Title,
                            Answer = CallAnswer.Title,
                            Date = CallAnswer.CreatedAt.ToString("MM/dd/yyyy HH:mm")
                        };
                    }).ToList(),
                    AllQuestions = _context.Questions.ToList()
                };
                return View(model);
            }
            catch (Exception e)
            {
                return Ok(e);
            }
        }

        [HttpGet("/call/{id:int}/information")]
        public async Task<IActionResult> CallInformation(int id, int page = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                ViewData["Search"] = search;
                ViewData["Page"] = page;
                ViewData["PageSize"] = pageSize;

                var call = _context.Calls
                    .Include(v => v.Customer)
                    .Include(v => v.Answers)
                    .FirstOrDefault(v => v.Id == id);

                var questions = _context.Questions.Where(v =>
                    v.DeletedAt == null && v.Answers.Any(answer => call.Answers.Contains(answer)));

                // search code
                if (search != null)
                {
                    // search by customer name or phone number or username
                    questions = _context.Questions.Where(v =>
                        v.Title != null && v.Title.Contains(search)
                    );
                }

                questions = questions
                    .Include(v => v.Answers);

                var items = await PaginationResponse<Question>.CreateAsync(questions.AsNoTracking(), page, pageSize);
                ViewData["TotalPage"] = items.TotalPages;

                var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
                var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;

                search ??= "";

                ViewData["PreviousPage"] = getCallInformationUrl(id: id, previousPage, pageSize, search);
                ViewData["NextPage"] = getCallInformationUrl(id: id, page = nextPage, pageSize, search);
                ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
                ViewData["EndRowNumber"] =
                    ((page * pageSize) < questions.Count()) ? (page * pageSize) : questions.Count();

                ViewData["TotalItem"] = questions.Count();
                var result = new QuestionViewModel
                {
                    CallId = id,
                    CustomerId = call.Customer.Id,
                    Questions = items.ConvertAll((item) => new QuestionResponse
                    {
                        Id = item.Id,
                        Question = item.Title,
                        AnswerCount = "الإجابات: " + (item.Answers != null ? item.Answers.Count.ToString() : "0")
                    })
                };
                return View(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        private static string getCallInformationUrl(int id, int page, int pageSize, string search)
        {
            return $"/call/{id}/information?page={page}&pageSize={pageSize}&search={search}";
        }

        [HttpGet("/call/{callId:int}/information/{id:int}")]
        public async Task<IActionResult> CallQuestionAnswers(int id, int callId, int page = 1, int pageSize = 10,
            string? search = null)
        {
            try
            {
                ViewData["Search"] = search;
                ViewData["Page"] = page;
                ViewData["PageSize"] = pageSize;
                var answers = _context.Answers
                    .Where(v => v.DeletedAt == null)
                    .Where(v => v.Question.Id == id);

                // search code
                if (search != null)
                {
                    // search by customer name or phone number or username
                    answers = answers.Where(v => v.Title != null && v.Title.Contains(search));
                }

                answers = answers
                    .Include(v => v.ReturnActions)
                    .Include(v => v.Question);

                var items = await PaginationResponse<Answer>.CreateAsync(answers.AsNoTracking(), page, pageSize);
                ViewData["TotalPage"] = items.TotalPages;

                var nextPage = items.HasNextPage ? items.PageIndex + 1 : items.TotalPages;
                var previousPage = items.HasPreviousPage ? items.PageIndex - 1 : 1;

                search ??= "";

                ViewData["PreviousPage"] = getCallAnswerUrl(id: id, callId: callId, previousPage, pageSize, search);
                ViewData["NextPage"] =
                    getCallAnswerUrl(id: id, callId: callId, page: nextPage, pageSize: pageSize, search);
                ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
                ViewData["EndRowNumber"] =
                    ((page * pageSize) < answers.Count()) ? (page * pageSize) : answers.Count();

                ViewData["TotalItem"] = answers.Count();
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                    return NotFound("السؤال غير موجود");

                var result = new AnswersViewModel
                {
                    Id = id,
                    Question = question.Title,
                    CanReturn = _context.Answers.Any(v => v.Title == null && v.Question.Id == id),
                    Answers = items.ConvertAll(item => new AnswerResponse
                    {
                        Id = item.Id,
                        Answer = item.Title,
                        Datetime = item.CreatedAt.ToString("dd/MM/yyyy hh:mm tt"),
                        Type = Helper.GetReturnActionType(_context, item)
                    }),
                    ReturnActions = _context.ReturnActions.ToList(),
                    CallId = callId
                };
                return View(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        private static string getCallAnswerUrl(int id, int callId, int page, int pageSize, string search)
        {
            return $"/call/{callId}/information/{id}?page={page}&pageSize={pageSize}&search={search}";
        }

        [HttpGet("/call/{id:int}/new")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> NewCustomer(int id)
        {
            var call = await _context.Calls.Include(v => v.Customer).FirstOrDefaultAsync(v => v.Id == id);
            var customer = call.Customer;
            if (customer.Name != null) return RedirectToAction("Calling", new { id = id });
            var model = new PotentialCustomerViewModel
            {
                Id = id,
                Phone = customer?.PhoneNumber ?? "N/A",
                ProjectTypes = _context.ProjectTypes.Select(v => new ProjectType
                {
                    Id = v.Id,
                    Name = v.Name
                }).ToList(),
                Cities = _context.Cities.Select(v => new City
                {
                    Id = v.Id,
                    Name = v.Name
                }).ToList(),
            };


            return View(model);
        }

        [HttpPost("/call/{id:int}/new")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> NewCustomer(NewCustomer model, int id)
        {
            var call = await _context.Calls.Include(v => v.Customer).FirstOrDefaultAsync(v => v.Id == id);

            var customer = call.Customer;
            customer.Name = model.CustomerName;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            var dateNow = DateTime.Now;

            var project = new Project
            {
                Customer = customer,
                Name = model.ProjectName,
                Owner = model.OwnerName,
                Address = model.Address,
                CreatedAt = dateNow,
                UpdatedAt = dateNow
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var city = await _context.Cities.FirstOrDefaultAsync(v => v.Name == model.City);
            if (city == null)
            {
                city = new City
                {
                    Name = model.City,
                    Project = project,
                    CreatedAt = dateNow,
                    UpdatedAt = dateNow
                };
                _context.Cities.Add(city);
                await _context.SaveChangesAsync();
            }

            var projectType = await _context.ProjectTypes.FirstOrDefaultAsync(v => v.Name == model.ProjectType);
            if (projectType == null)
            {
                projectType = new ProjectType
                {
                    Name = model.ProjectType,
                    Project = project,
                    CreatedAt = dateNow,
                    UpdatedAt = dateNow
                };
                _context.ProjectTypes.Add(projectType);
                await _context.SaveChangesAsync();
            }

            const string message = "تم إضافة العميل بنجاح";
            const string title = "تمت العملية بنجاح";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Success);
            return RedirectToAction("Calling", new { id = call.Id });
        }


        [HttpGet("/call/{id:int}/end")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> EndCall(int id)
        {
            try
            {
                var call = await _context.Calls.FindAsync(id);
                call.End = DateTime.Now;
                _context.Calls.Update(call);
                await _context.SaveChangesAsync();

                const string message = "تم إنهاء المكالمة بنجاح";
                const string title = "تمت العملية بنجاح";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Success);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}