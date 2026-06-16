public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByCodeAsync(string code);
}