using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Tally.Application;
using Tally.Domain;

namespace Tally.Api.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FilesController(FileService fileService) : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<Domain.FileResult>> Upload([Required] IFormFile file, [FromQuery, Required] string column)
    {
        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Only .csv files are supported");
        }

        await using var stream = file.OpenReadStream();
        var result = await fileService.ProcessAsync(stream, Path.GetFileName(file.FileName), column);
        return Ok(result);
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ProcessedFile>> GetHistory() => Ok(fileService.GetHistory());

    [HttpGet("summary")]
    public ActionResult<FileSummary> GetSummary() => fileService.GetSummary();
}
