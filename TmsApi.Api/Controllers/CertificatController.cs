using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificateController(
    ICertificatServices certificateService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get all certificates")]
    [ProducesResponseType(typeof(List<CertificatResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        return Ok(await certificateService.GetAllAsync(ct));
    }

    [HttpGet("{id:int}", Name = nameof(GetCertificateById))]
    [EndpointSummary("Get certificate by id")]
    [ProducesResponseType(typeof(CertificateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCertificateById(int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(id, ct);

        if (certificate is null)
            return NotFound();

        var self = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCertificateById),
            new { id });

        var links = new List<LinkDto>
        {
            new(self!, "self", "GET"),
            new(self!, "update", "PUT"),
            new(self!, "delete", "DELETE")
        };   

        var dto = new CertificateDetailDto(
            certificate.Id,
            certificate.SerialNumber,
            certificate.Title,
            DateTime.UtcNow,
            certificate.StudentId,
            certificate.CourseId,
            links);

        return Ok(dto);
    }

    [HttpPost]
    [EndpointSummary("Create certificate")]
    [ProducesResponseType(typeof(CertificatResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
      
            CreateCertificateRequest request,
        CancellationToken ct)
    {
        var certificate = await certificateService.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetCertificateById),
            new { id = certificate.Id },
            certificate);
    }

    [HttpPut("{id:int}")]
    [EndpointSummary("Update certificate")]
    [ProducesResponseType(typeof(CertificatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        UpdateCertificateRequest request,
        CancellationToken ct)
    {
        var certificate = await certificateService.UpdateAsync(id, request, ct);

        if (certificate is null)
            return NotFound();

        return Ok(certificate);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Delete certificate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var deleted = await certificateService.DeleteAsync(id, ct);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}