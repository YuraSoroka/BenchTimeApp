namespace BenchTimeApp.Library.DataAccess;

public interface IStatusData
{
    Task<List<StatusModel>> GetAllStatuses();
    Task CreateModel(StatusModel status);
}