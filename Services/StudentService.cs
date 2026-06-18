namespace TmsApi.Entities;

public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<Student> GetByIdAsync(string id);
}

public class StudentService : IStudentService
{


    public Task<List<Student>> GetAllAsync()
        => Task.FromResult(new List<Student>(){
            new Student(){ Id=1, Name="John Doe" ,RegistrationNumber="REG-001"}});

    public Task<Student> GetByIdAsync(string id)
        => Task.FromResult(
           new Student() { Id = 1, Name = "John Doe", RegistrationNumber = "REG-001" });
}