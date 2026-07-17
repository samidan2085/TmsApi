namespace TmsApi.Dtos;
public record UpdateCourseRequest
{
   public required  string Code{get; init;}
    public required string  Title{get; init;}
 public required    int MaxCapacity{get; init;}
};