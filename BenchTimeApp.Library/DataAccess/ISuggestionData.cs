namespace BenchTimeApp.Library.DataAccess;

public interface ISuggestionData
{
    Task<List<SuggestionModel>> GetAllSuggestions();
    Task<List<SuggestionModel>> GetAllApprovedSuggestions();
    Task<SuggestionModel> GetSuggestion(string id);
    Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApprove();
    Task UpdateSuggestion(SuggestionModel suggestion);
    Task UpvoteSuggestion(string suggestionId, string userId);
    Task CreateSuggestion(SuggestionModel suggestion);
}