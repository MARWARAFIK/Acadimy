using Acadimy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Acadimy.Models;
using Microsoft.AspNetCore.Mvc;

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

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            // Simple AI processing
            var result = content.Length > 300
                ? content.Substring(0, 300) + "..."
                : content;

            return Json(new
            {
                answer = "📄 Résumé du fichier :<br/>" + result
            });
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