using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class DocumentController(IRepositoryManager repositoryManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> View([FromQuery] int id, CancellationToken cancellationToken)
    {
        var doc = await repositoryManager.DocumentRepository
            .FindByCondition(d => d.Id == id, false)
            .SingleOrDefaultAsync(cancellationToken);

        if (doc == null || doc.Content == null || doc.Content.Length == 0)
        {
            return NotFound();
        }

        var contentType = doc.ContentType ?? "application/octet-stream";
        return File(doc.Content, contentType);
    }

    [HttpGet]
    public async Task<IActionResult> Download([FromQuery] int id, CancellationToken cancellationToken)
    {
        var doc = await repositoryManager.DocumentRepository
            .FindByCondition(d => d.Id == id, false)
            .SingleOrDefaultAsync(cancellationToken);

        if (doc == null || doc.Content == null || doc.Content.Length == 0)
        {
            return NotFound();
        }

        var contentType = doc.ContentType ?? "application/octet-stream";
        var fileName = string.IsNullOrWhiteSpace(doc.Name) ? $"document_{doc.Id}.{doc.Extension}" : doc.Name;
        return File(doc.Content, contentType, fileName);
    }
}
