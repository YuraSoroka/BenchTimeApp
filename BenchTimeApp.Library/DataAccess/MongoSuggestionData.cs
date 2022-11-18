using Microsoft.Extensions.Caching.Memory;

namespace BenchTimeApp.Library.DataAccess;

public class MongoSuggestionData : ISuggestionData
{
    private readonly IDbConnection _db;
    private readonly IUserData _userData;
    private readonly IMemoryCache _cache;

    private readonly IMongoCollection<SuggestionModel> _suggestions;
    private const string CacheName = "SuggestionData";

    public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
    {
        _db = db;
        _userData = userData;
        _cache = cache;
        _suggestions = db.SuggestionCollection;
    }

    public async Task<List<SuggestionModel>> GetAllSuggestions()
    {
        var output = _cache.Get<List<SuggestionModel>>(CacheName);
        if (output is null)
        {
            var results = await _suggestions.FindAsync(suggestion => suggestion.Archived == false);
            output = results.ToList();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
        }
        
        return output;
    }

    public async Task<List<SuggestionModel>> GetAllApprovedSuggestions()
    {
        var output = await GetAllSuggestions();
        return output.Where(suggestion => suggestion.Approved).ToList();
    }

    public async Task<SuggestionModel> GetSuggestion(string id)
    {
        var result = await _suggestions.FindAsync(suggestion => suggestion.Id == id);
        return result.FirstOrDefault();
    }

    public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApprove()
    {
        var output = await GetAllSuggestions();
        return output.Where(x => x.Approved == false && x.Rejected == false).ToList();
    }

    public async Task UpdateSuggestion(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _cache.Remove(CacheName);
    }

    public async Task UpvoteSuggestion(string suggestionId, string userId)
    {
        var client = _db.Client;
        using (var session = await client.StartSessionAsync())
        {
            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
                var suggestion = (await suggestionsInTransaction.FindAsync(s => s.Id == suggestionId)).First(); // one suggested model

                bool isUpvote = suggestion.UserVotes.Add(userId);
                if (isUpvote == false)
                {
                    suggestion.UserVotes.Remove(userId);
                }

                await suggestionsInTransaction.ReplaceOneAsync(s => s.Id == suggestionId, suggestion); // replace with updated votes

                var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _userData.GetUserAsync(suggestion.Author.Id); // get user, who suggested

                // if user votes - add voted suggestionId to his votes
                if (isUpvote) 
                { 
                    user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
                }
                else
                {
                    var suggestionsToRemove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
                    user.VotedOnSuggestions.Remove(suggestionsToRemove);
                }

                await usersInTransaction.ReplaceOneAsync(u => u.Id == userId, user);

                await session.CommitTransactionAsync();
                
                _cache.Remove(CacheName);
            }
            catch (Exception e)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }

    public async Task CreateSuggestion(SuggestionModel suggestion)
    {
        var client = _db.Client;
        using (var session = await client.StartSessionAsync())
        {
            session.StartTransaction();
            try
            {
                var db = client.GetDatabase(_db.DbName);
                var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
                await suggestionsInTransaction.InsertOneAsync(suggestion);

                var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _userData.GetUserAsync(suggestion.Author.Id);
                user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));
                await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

                await session.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}