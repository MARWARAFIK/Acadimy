using Acadimy.Models;
using Acadimy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;

namespace Acadimy.Controllers
{
    [Authorize]
    public class AiAssistantController : Controller
    {
        private readonly IAiAssistantService _aiService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AiAssistantController(
            IAiAssistantService aiService,
            UserManager<ApplicationUser> userManager)
        {
            _aiService = aiService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { answer = "Aucun fichier reçu." });

            string extractedText = "";
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension == ".pdf")
            {
                using var stream = file.OpenReadStream();
                using var pdf = PdfDocument.Open(stream);

                foreach (var page in pdf.GetPages())
                {
                    extractedText += page.Text + "\n";
                }
            }
            else if (extension == ".txt")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                extractedText = await reader.ReadToEndAsync();
            }
            else
            {
                return Json(new { answer = "Format non supporté. Utilise PDF ou TXT." });
            }

            if (string.IsNullOrWhiteSpace(extractedText))
                return Json(new { answer = "Impossible de lire le contenu." });

            if (extractedText.Length > 4000)
                extractedText = extractedText.Substring(0, 4000);

            var userId = _userManager.GetUserId(User);

            var prompt = "Fais un résumé clair de ce document:\n\n" + extractedText;

            var result = await _aiService.AskAsync(userId!, prompt);

            return Json(new { answer = result });
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AiAskRequest request)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var response = await _aiService.AskAsync(userId, request.Message);

            return Json(new { answer = response });
        }
    }

    public class AiAskRequest
    {
        public string Message { get; set; } = "";
    }
}