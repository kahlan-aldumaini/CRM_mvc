using System.Data;
using ClosedXML.Excel;
using CRM_mvc.Context;
using CRM_mvc.Models.Entities;
using CRM_mvc.Models.Views.QuestionsAndAnswer;
using CRM_mvc.Utilities;
using CRM_mvc.Utilities.Config;
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
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuestionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                ViewData["Search"] = search;
                ViewData["Page"] = page;
                ViewData["PageSize"] = pageSize;

                var questions = _context.Questions.Where(v => v.DeletedAt == null);

                var user = await _userManager.GetUserAsync(User);
                if (user != null && await _userManager.IsInRoleAsync(user, "USER"))
                {
                    questions = questions.Where(ques => ques.Answers.Any(a => a.User.Id == user.Id));
                }

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

                ViewData["PreviousPage"] = getQuestionUrl(previousPage, pageSize, search);
                ViewData["NextPage"] = getQuestionUrl(page = nextPage, pageSize, search);
                ViewData["StartRowNumber"] = (page * pageSize) - pageSize;
                ViewData["EndRowNumber"] =
                    ((page * pageSize) < questions.Count()) ? (page * pageSize) : questions.Count();

                ViewData["TotalItem"] = questions.Count();

                var getAnswersCount = (IEnumerable<Answer> answer) => $"الاجابات: {items.Count} ";
                var result = new QuestionViewModel
                {
                    Questions = items.ConvertAll((item) => new QuestionResponse
                    {
                        Id = item.Id,
                        Question = item.Title,
                        AnswerCount = getAnswersCount(item.Answers.Where(a => a.User == user))
                    })
                };
                return View(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        private static string getQuestionUrl(int page, int pageSize, string search)
        {
            return $"/question?page={page}&pageSize={pageSize}&search={search}";
        }

        #region ExportQuestion

        [HttpGet("/question/export-to-excel")]
        public FileResult ExportToExcel()
        {
            var questions = _context.Questions.Where(v => v.DeletedAt == null)
                .Include(question => question.Answers)
                .ToList();
            var fileName = "questions.xlsx";
            return GenerateExcel(fileName, questions);
        }

        private FileResult GenerateExcel(string fileName, IEnumerable<Question> questions)
        {
            var dataTable = new DataTable("Question");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("السؤال"),
            });
            foreach (var item in questions)
            {
                DataRow row = dataTable.NewRow();
                row["السؤال"] = item.Title;
                var index = 1;
                foreach (var answer in item.Answers)
                {
                    if (!dataTable.Columns.Contains($"الاجابة {index}"))
                        dataTable.Columns.Add($"الاجابة {index}");
                    row[$"الاجابة {index}"] = answer.Title;
                    index++;
                }

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

        [HttpGet("question/answers/{id:int}", Name = "Answers")]
        public async Task<IActionResult> Answers(int id, int page = 1, int pageSize = 10, string? search = null,
            string? emp = null)
        {
            try
            {
                ViewData["Search"] = search;
                ViewData["Page"] = page;
                ViewData["PageSize"] = pageSize;
                var answers = _context.Answers
                    .Where(v => v.DeletedAt == null)
                    .Where(v => v.Question.Id == id);

                var user = await _userManager.GetUserAsync(User);
                if (await _userManager.IsInRoleAsync(user, "USER"))
                {
                    answers = answers.Where(answer => answer.User.Id == user.Id);
                }

                if (emp != null)
                {
                    answers = answers.Where(answer => answer.User.Id == emp);
                    ViewData["User"] = emp;
                }

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

                ViewData["PreviousPage"] = getAnswerUrl(id: id, previousPage, pageSize, search);
                ViewData["NextPage"] = getAnswerUrl(id: id, page = nextPage, pageSize, search);
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
                    ReturnActions = _context.ReturnActions.ToList()
                };
                return View(result);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        private static string getAnswerUrl(int id, int page, int pageSize, string search)
        {
            return $"/question/answers/{id}?page={page}&pageSize={pageSize}&search={search}";
        }

        #region ExportAnswerExcel

        [HttpGet("/question/answers/{id:int}/export-to-excel")]
        public async Task<FileResult> ExportToExcel(int id, string? emp = null)
        {
            var answers = _context.Answers
                .Include(v=>v.ReturnActions)
                .Include(answer => answer.User)
                .Include(answer => answer.Question)
                .Where(answer => answer.Question.Id == id && answer.DeletedAt == null);

            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "USER"))
            {
                answers = answers.Where(answer => answer.User.Id == user.Id);
            }

            if (emp != null)
            {
                answers = answers.Where(answer => answer.User.Id == emp);
                ViewData["User"] = emp;
            }

            return GenerateExcel("Answers.xlsx", answers.ToList());
        }

        private FileResult GenerateExcel(string fileName, IEnumerable<Answer> answers)
        {
            var dataTable = new DataTable("Question");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new("السؤال"),
                new("الاجابة"),
                new("الموظف"),
                new("تاريخ الاجابة"),
                new("نوع الرد")
            });
            foreach (var item in answers)
            {
                var row = dataTable.NewRow();
                row["السؤال"] = _context.Questions.FirstOrDefault(v => v.Answers.Any(a => a.Id == item.Id))?.Title;
                row["الاجابة"] = item.Title;
                row["الموظف"] = item.User.Name;
                row["تاريخ الاجابة"] = item.CreatedAt.ToString("MM/dd/yyyy");
                row["نوع الرد"] = item.ReturnActions.FirstOrDefault()?.Title;
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

        [HttpPost("/question/store/answer/{id:int}")]
        [Authorize(Roles = "USER")]
        public async Task<IActionResult> EditAnswers(NewAnswer model, int id)
        {
            if (ModelState.IsValid)
            {
                var question = await _context.Questions.Include(v => v.Answers).FirstOrDefaultAsync(v => v.Id == id);
                if (question == null)
                {
                    const string message = "السؤال غير موجود";
                    const string title = "خطأ";
                    Helper.SendNotification(context: HttpContext, title: title, message: message,
                        type: NotifyType.Error);

                    return RedirectToAction("Answers", new { id });
                }

                var answer = new Answer
                {
                    Title = model.Answer,
                    Question = question,
                    User = await _userManager.GetUserAsync(User),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null
                };
                await _context.Answers.AddAsync(answer);
                await _context.SaveChangesAsync();

                if (model.ReturnAction != null)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var oldAnswer =
                        await _context.Answers.FirstOrDefaultAsync(v => v.Title == null && v.Question.Id == id);


                    var callAnswers =
                        await _context.CallAnswers.FirstOrDefaultAsync(v =>
                            v.AnswerId == oldAnswer.Id && v.DeletedAt == null);
                    if (callAnswers == null)
                        return NotFound("السؤال غير موجود");

                    var receiver =
                        await _context.Customers.FirstOrDefaultAsync(v => v.Calls.Any(v => v.Id == callAnswers.CallId));
                    if (receiver == null)
                        return NotFound("المستخدم غير موجود");

                    var isDone = false;

                    var returnAction =
                        await _context.ReturnActions.FirstOrDefaultAsync(v => v.Title == model.ReturnAction);
                    if (returnAction == null)
                    {
                        returnAction = new ReturnAction
                        {
                            Title = model.ReturnAction,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        await _context.ReturnActions.AddAsync(returnAction);
                        await _context.SaveChangesAsync();
                    }

                    if (returnAction.Title == ReturnActionType.EMAIL.ToString())
                    {
                        const string subject = "رد على سؤالك";
                        var body = "تم الرد على سؤالك بواسطة " + user.Name;

                        try
                        {
                            var client = Mail.setClint();
                            await Helper.sendEmail(_context: _context, receiver: receiver.Id.ToString(), user: user,
                                subject: subject, body: body, from: user.Email, client: client);
                            isDone = true;
                        }
                        catch (Exception e)
                        {
                            return BadRequest(e);
                        }
                    }

                    //TODO:send sms

                    callAnswers.AnswerId = answer.Id;
                    _context.CallAnswers.Update(callAnswers);

                    var answerReturnAction = new AnswerReturnAction
                    {
                        DateTime = model.DateTime ?? DateTime.Now,
                        AnswerId = answer.Id,
                        ReturnActionId = returnAction.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsDone = isDone,
                    };
                    await _context.AnswerReturnActions.AddAsync(answerReturnAction);

                    _context.Answers.Remove(oldAnswer);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                const string message = "الرجاء التأكد من البيانات المدخلة";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);

                return BadRequest(ModelState);
            }

            const string message1 = "تمت العملية بنجاح";
            const string title1 = "نجاح";
            Helper.SendNotification(context: HttpContext, title: title1, message: message1, type: NotifyType.Success);
            return RedirectToAction("Answers", new { id = id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteAnswer(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer == null) return RedirectToAction("Answers");
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            const string message = "تمت العملية بنجاح";
            const string title = "نجاح";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Success);

            return RedirectToAction("Answers");
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null) return RedirectToAction("Index");
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            const string message = "تمت العملية بنجاح";
            const string title = "نجاح";
            Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Success);

            return RedirectToAction("Index");
        }

        [HttpPost("/question/call/add/{id:int}", Name = "AddQuestion")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> AddCallQuestion(NewQuestionViewModel model, int id)
        {
            var hasAnswer = model.hasAnswer == "on";

            var call = await _context.Calls.FindAsync(id);
            if (call == null)
            {
                const string message = "المكالمة غير موجودة";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);

                return RedirectToAction("Calling", "Call", new { id = id });
            }

            var question = await _context.Questions
                .Include(v => v.Answers)
                .FirstOrDefaultAsync(v => v.Title == model.Question);

            if (question == null)
            {
                question = new Question
                {
                    Title = model.Question,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
            }

            var answer = new Answer
            {
                Title = hasAnswer ? model.Answer : null,
                Question = question,
                User = await _userManager.GetUserAsync(User),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();


            // updateCall 
            var nowDate = DateTime.Now;
            var callAnswers = new CallAnswers
            {
                AnswerId = answer.Id,
                CallId = call.Id,
                CreatedAt = nowDate,
                UpdatedAt = nowDate,
            };

            _context.CallAnswers.Add(callAnswers);
            await _context.SaveChangesAsync();

            const string message1 = "تمت العملية بنجاح";
            const string title1 = "نجاح";
            Helper.SendNotification(context: HttpContext, title: title1, message: message1, type: NotifyType.Success);

            return Redirect($"/call/{id}");
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EditQuestion(EditQuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                const string message = "الرجاء التأكد من البيانات المدخلة";
                const string title = "خطأ";
                Helper.SendNotification(context: HttpContext, title: title, message: message, type: NotifyType.Error);

                return RedirectToAction("Index");
            }

            var question = await _context.Questions.FindAsync(model.Id);
            if (question != null)
            {
                question.Title = model.Question;
                _context.Questions.Update(question);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newQuestion = new Question
                {
                    Title = model.Question
                };
                _context.Questions.Add(newQuestion);
                await _context.SaveChangesAsync();
            }

            const string message1 = "تمت العملية بنجاح";
            const string title1 = "نجاح";
            Helper.SendNotification(context: HttpContext, title: title1, message: message1, type: NotifyType.Success);

            return RedirectToAction("Index");
        }
    }
}