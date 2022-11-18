namespace BenchTimeApp.Library.DataAccess;

public class MongoUserData : IUserData
{
    private IMongoCollection<UserModel> _users;

    public MongoUserData(IDbConnection db)
    {
        _users = db.UserCollection;
    }

    public async Task<List<UserModel>> GetUsersAsync()
    {
        var results = await _users.FindAsync(_ => true);
        return results.ToList();
    }

    public async Task<UserModel> GetUserAsync(string id)
    {
        var result = await _users.FindAsync(user => user.Id == id);
        return result.FirstOrDefault();
    }
    
    public async Task<UserModel> GetUserFromAuthenticationAsync(string objectId)
    {
        var result = await _users.FindAsync(user => user.ObjectIdentifier == objectId);
        return result.FirstOrDefault();
    }

    public Task CreateUser(UserModel user)
    {
        return _users.InsertOneAsync(user);
    }

    public Task UpdateUser(UserModel user)
    {
        var filter = Builders<UserModel>.Filter.Eq("Id", user.Id);
        return _users.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
    }
}
