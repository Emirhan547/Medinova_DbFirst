using Medinova.Attributes;
using Medinova.Helpers;
using Medinova.Models;
using Medinova.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Medinova.Areas.Patient.Controllers
{
    [CustomAuthorize("Patient")]
    public class AIAssistantController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AIService aiService;
        private readonly string apiKey;

        public AIAssistantController()
        {
            // 🔥 ARTIK web.config YOK → Environment Variable VAR
            apiKey = Env.Get("MEDINOVA_ANTHROPIC_API_KEY", required: false);

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                aiService = new AIService(apiKey);
            }
        }

        public ActionResult Index()
        {
            var userId = (int)Session["userId"];

            var conversations = context.AIConversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .Take(20)
                .ToList();

            return View(conversations);
        }

        [HttpPost]
        public async Task<JsonResult> AskQuestion(string question)
        {
            try
            {
                if (aiService == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "AI servisi yapılandırılamadı. Lütfen API anahtarını kontrol edin."
                    });
                }

                if (string.IsNullOrWhiteSpace(question))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lütfen geçerli bir soru girin."
                    });
                }

                var userId = (int)Session["userId"];

                var departmentNames = context.Departments
                     .Select(d => d.Name)
                     .Where(name => name != null && name != "")
                     .AsEnumerable()
                     .Select(name => name.Trim())
                     .Where(name => !string.IsNullOrWhiteSpace(name))
                     .Distinct()
                     .ToList();

                var aiResponse = departmentNames.Any()
                    ? await aiService.GetPatientAssistantResponse(question, departmentNames)
                    : await aiService.GetHealthAdvice(question);

                var conversation = new AIConversation
                {
                    UserId = userId,
                    UserMessage = question,
                    AIResponse = aiResponse,
                    CreatedDate = DateTime.Now
                };

                context.AIConversations.Add(conversation);
                context.SaveChanges();

                return Json(new
                {
                    success = true,
                    response = aiResponse
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Bir hata oluştu: " + ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetDepartmentSuggestion(string symptoms)
        {
            try
            {
                if (aiService == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "AI servisi yapılandırılamadı. Lütfen API anahtarını kontrol edin."
                    });
                }

                if (string.IsNullOrWhiteSpace(symptoms))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lütfen şikayetinizi girin."
                    });
                }

                var departmentNames = context.Departments
                    .Select(d => d.Name)
                    .Where(name => name != null && name != "")
                    .AsEnumerable()
                    .Select(name => name.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .ToList();

                if (!departmentNames.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Departman listesi bulunamadı. Lütfen daha sonra tekrar deneyin."
                    });
                }

                var recommendation =
                    await aiService.GetDepartmentRecommendation(symptoms, departmentNames);

                return Json(new
                {
                    success = true,
                    recommendation
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
