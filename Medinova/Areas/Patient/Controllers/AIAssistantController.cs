using Medinova.Attributes;
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

        public AIAssistantController()
        {
            // API Key'i web.config'den alın
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["AnthropicApiKey"];
            aiService = new AIService(apiKey);
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
                var userId = (int)Session["userId"];

                // Get AI response
                var aiResponse = await aiService.GetHealthAdvice(question);

                // Save to database
                var conversation = new AIConversation
                {
                    UserId = userId,
                    UserMessage = question,
                    AIResponse = aiResponse,
                    CreatedDate = DateTime.Now
                };

                context.AIConversations.Add(conversation);
                context.SaveChanges();

                return Json(new { success = true, response = aiResponse });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetDepartmentSuggestion(string symptoms)
        {
            try
            {
                var departmentNames = context.Departments
                    .Select(department => department.Name)
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

                var recommendation = await aiService.GetDepartmentRecommendation(symptoms, departmentNames);
                return Json(new { success = true, recommendation });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}