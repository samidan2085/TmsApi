using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly TmsDbContext _context;
    private readonly LinkGenerator _linkGenerator;

    public StudentsController(
        IStudentService studentService,
        TmsDbContext context,
        LinkGenerator linkGenerator)
    {
        _studentService = studentService;
        _context = context;
        _linkGenerator = linkGenerator;
    }

    //======================
    // GET ALL
    //======================

    [HttpGet]
    [EndpointSummary("Get all students")]
    [EndpointDescription("Returns all students.")]
    [ProducesResponseType(typeof(List<StudentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var students = await _studentService.GetAllAsync(ct);

        return Ok(students);
    }

    //======================
    // GET BY ID
    //======================

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [EndpointSummary("Get student by Id")]
    [EndpointDescription("Returns a student together with HATEOAS links.")]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById(
        int id,
        CancellationToken ct)
    {
        var student = await _studentService.GetByIdAsync(id,ct);

        if (student is null)
            return NotFound();

        var self = _linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetStudentById),
            new { id });

        var links = new List<LinkDto>
        {
            new(self!, "self", "GET"),
            new(self!, "update", "PUT"),
            new(self!, "delete", "DELETE")
        };

        var dto = new StudentDetailDto(
            student.Id,
            student.RegistrationNumber,
            student.Name,
            student.Age,
            student.GPA,
            links);

        return Ok(dto);
    }

    //======================
    // PAGINATION
    //======================

    [HttpGet("small-page-response")]
    [EndpointSummary("Get students with pagination")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudents(
        int pageNumber = 1,
        int pageSize = 5,
        CancellationToken ct = default)
    {
        var students = await _context.Students
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return Ok(students);
    }

    //======================
    // CREATE
    //======================

    [HttpPost]
    [EndpointSummary("Create Student")]
    [EndpointDescription("Creates a new student.")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStudent(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = await _studentService.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetStudentById),
            new { id = student.Id },
            student);
    }

    //======================
    // UPDATE
    //======================

    [HttpPut("{id:int}")]
    [EndpointSummary("Update Student")]
    [EndpointDescription("Updates an existing student.")]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(
        int id,
        UpdateStudentRequest request,
        CancellationToken ct)
    {
        var student = await _studentService.UpdateAsync(id, request, ct);

        if (student is null)
            return NotFound();

        return Ok(student);
    }

    //======================
    // DELETE
    //======================

    [HttpDelete("{id:int}")]
    [EndpointSummary("Delete Student")]
    [EndpointDescription("Deletes a student.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudent(
        int id,
        CancellationToken ct)
    {
        var deleted = await _studentService.DeleteAsync(id, ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}