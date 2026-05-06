using Acadimy.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Acadimy.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public AiAssistantService(
            ApplicationDbContext context,
            IConfiguration config,
            HttpClient http)
        {
            _context = context;
            _config = config;
            _http = http;
        }

        public async Task<string> AskAsync(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Pose-moi une question 😊";

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return "Utilisateur introuvable.";

            var apiKey = _config["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                return "Clé Groq manquante.";

            var body = new
            {
                model = "llama-3.1-8b-instant",

                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = $@"
Tu es l'assistant IA de Acadimy.

Utilisateur:
Nom: {user.FullName}
Filière: {user.Filiere}
Niveau: {user.Niveau}

Réponds en français ou darija.
Tu aides avec:
- ASP.NET
- programmation
- projets
- cours
- résumé
- correction
- questions générales

Utilise HTML simple:
<br>, <b>, <ul>, <li>
"
                    },

                    new
                    {
                        role = "user",
                        content = message
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions"
            );

            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);

            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return "Erreur Groq : " + responseText;

            using var doc = JsonDocument.Parse(responseText);

            var answer = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return answer ?? "Pas de réponse.";
        }
    }
}