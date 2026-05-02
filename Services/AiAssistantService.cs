using Acadimy.Data;
using Acadimy.Models.Teacher;
using Microsoft.EntityFrameworkCore;

namespace Acadimy.Services
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly ApplicationDbContext _context;

        public AiAssistantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> AskAsync(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Pose-moi une question sur les cours, projets ou contenus pédagogiques.";

            var msg = message.ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return "Utilisateur introuvable.";

            if (msg.Contains("cours") || msg.Contains("course") || msg.Contains("recommande"))
            {
                var courses = await _context.TeacherCourses
           .Where(c =>
               (!string.IsNullOrEmpty(user.Filiere) &&
                !string.IsNullOrEmpty(c.Category) &&
                c.Category.Contains(user.Filiere)) ||

               (!string.IsNullOrEmpty(user.Niveau) &&
                !string.IsNullOrEmpty(c.Level) &&
                c.Level.Contains(user.Niveau)) ||

               (!string.IsNullOrEmpty(user.Skill) &&
                !string.IsNullOrEmpty(c.Title) &&
                c.Title.Contains(user.Skill)))
           .OrderByDescending(c => c.CreatedAt)
           .Take(5)
           .ToListAsync();

                if (!courses.Any())
                    return "Aucun cours disponible pour le moment.";

                var result = "Je te recommande ces cours :<br/>";

                foreach (var c in courses)
                {
                    result += $"• <b>{c.Title}</b> - {c.Category} / {c.Level}<br/>";
                }

                return result;
            }

            if (msg.Contains("résumé") || msg.Contains("resume") || msg.Contains("summarize"))
            {
                return "Résumé automatique : ce contenu explique les idées principales, les objectifs pédagogiques, et les points importants à retenir. Pour un meilleur résumé, colle-moi le texte du cours.";
            }

            if (msg.Contains("projet") || msg.Contains("project"))
            {
                return "Aide projet : explique clairement l’objectif, les technologies utilisées, les fonctionnalités principales, puis ajoute des améliorations possibles comme recherche, filtres, commentaires ou statistiques.";
            }

            if (msg.Contains("corrige") || msg.Contains("correction") || msg.Contains("texte"))
            {
                return "Assistance enseignant : je peux aider à corriger, reformuler et améliorer un texte. Colle-moi le texte que tu veux corriger.";
            }

            if (msg.Contains("bonjour") || msg.Contains("salut") || msg.Contains("hello"))
            {
                return $"Bonjour {user.FullName} 👋 Je suis ton assistant IA. Tu peux me demander une recommandation de cours, un résumé, ou de l’aide pour un projet.";
            }

            return "Je peux t’aider avec :<br/>• Recommandation de cours<br/>• Résumé de contenu<br/>• Aide projet<br/>• Correction de texte";
        }
    }
}