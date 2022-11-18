namespace BenchTimeApp.Library.DataAccess;

public interface IUserData
{
    Task<List<UserModel>> GetUsersAsync();
    Task<UserModel> GetUserAsync(string id);
    Task<UserModel> GetUserFromAuthenticationAsync(string objectId);
    Task CreateUser(UserModel user);
    Task UpdateUser(UserModel user);
}