using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Medinova.Helpers;

namespace Medinova.Services
{
    public class OpenAiSearchClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;

        public OpenAiSearchClient()
        {
            _apiKey = Env.Get("MEDINOVA_OPENAI_API_KEY");
            _baseUrl = Env.Get("MEDINOVA_OPENAI_BASE_URL");
            _model = Env.Get("MEDINOVA_OPENAI_MODEL", required: false) ?? "gpt-4o-mini";

            // Header sadece 1 kez eklenir
            if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"Bearer {_apiKey}"
                );
            }
        }

        public async Task<string> GetPopularAsync(string context)
        {
            var payload = new
            {
                model = _model,
                input = context,
                temperature = 0.2
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/responses",
                content
            );

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
