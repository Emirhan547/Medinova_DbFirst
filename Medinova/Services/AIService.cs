using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medinova.Services
{
    public class AIService
    {
        private readonly AnthropicClient _client;

        public AIService(string apiKey)
        {
            _client = new AnthropicClient(apiKey);
        }

        public async Task<string> GetDepartmentRecommendation(string symptoms, IReadOnlyCollection<string> departmentNames)
        {
            var departmentsList = string.Join(", ", departmentNames);
            var systemPrompt = $@"Sen bir sağlık asistanısın. Hastaların belirtilerine göre hangi hastane bölümüne gitmeleri gerektiğini öneriyorsun.
Mevcut bölümler: {departmentsList}.
Önerini yalnızca bu listede yer alan bölümler üzerinden yap.
Kısa ve net öneride bulun. Acil durumları belirt.";

            var messages = new List<Message>
    {
        new Message(RoleType.User, symptoms)
    };

            var parameters = new MessageParameters
            {
                Messages = messages,
                Model = "claude-sonnet-4-20250514",
                MaxTokens = 500,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);

            var text = response.Content
                .OfType<TextContent>()
                .FirstOrDefault();

            return text?.Text ?? string.Empty;
        }


        public async Task<string> GetHealthAdvice(string question)
        {
            var systemPrompt = @"Sen yardımsever bir sağlık asistanısın. Genel sağlık soruları hakkında bilgi veriyorsun.
ÖNEMLİ: Tıbbi teşhis koyma, ilaç önerme. Sadece genel bilgi ver ve doktora başvurmayı öner.
Kısa, anlaşılır Türkçe cevap ver.";

            var messages = new List<Message>
    {
        new Message(RoleType.User, question)
    };

            var parameters = new MessageParameters
            {
                Messages = messages,
                Model = "claude-sonnet-4-20250514",
                MaxTokens = 800,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);

            var text = response.Content
                .OfType<TextContent>()
                .FirstOrDefault();

            return text?.Text ?? string.Empty;
        }
        public async Task<string> GetPatientAssistantResponse(string question, IReadOnlyCollection<string> departmentNames)
        {
            var departmentsList = string.Join(", ", departmentNames);
            var systemPrompt = $@"Sen yardımsever bir sağlık asistanısın. Kullanıcı semptomlarını (ör. ''karnım ağrıyor'') yazdığında:
1) Kısa ve anlaşılır genel öneriler ver.
2) Uygun hastane bölümünü öner ve 'Önerilen bölüm:' ifadesiyle belirt.
3) Acil durum belirtilerinde 112 veya acil servise yönlendir.
Tıbbi teşhis koyma ve ilaç önerme.
Mevcut bölümler: {departmentsList}.
Önerini yalnızca bu listede yer alan bölümler üzerinden yap.
Yanıtın Türkçe, kısa ve sohbet tarzında olsun.";

            var messages = new List<Message>
            {
                new Message(RoleType.User, question)
            };

            var parameters = new MessageParameters
            {
                Messages = messages,
                Model = "claude-sonnet-4-20250514",
                MaxTokens = 900,
                System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
            };

            var response = await _client.Messages.GetClaudeMessageAsync(parameters);

            var text = response.Content
                .OfType<TextContent>()
                .FirstOrDefault();

            return text?.Text ?? string.Empty;
        }
    }
}