using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.Document.AddDocument;
using ZasNet.Application.UseCases.Queries.Document.DownloadDocument;
using ZasNet.Application.UseCases.Queries.Document.ViewDocument;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class DocumentController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<FileContentResult> View([FromQuery] ViewDocumentRequest viewDocumentRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(viewDocumentRequest, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Download([FromQuery] DownloadDocumentRequest downloadDocumentRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(downloadDocumentRequest, cancellationToken);
    }

    [HttpPost]
    public async Task AddDocument(
        [FromForm] AddDocumentCommand addDocumentCommand,
        CancellationToken cancellationToken)
    {
        await mediator.Send(addDocumentCommand, cancellationToken);
    }
}
