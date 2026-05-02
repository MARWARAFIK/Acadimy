namespace Acadimy.Services
{
    public interface IAiAssistantService
    {
        Task<string> AskAsync(string userId, string message);
    }
}