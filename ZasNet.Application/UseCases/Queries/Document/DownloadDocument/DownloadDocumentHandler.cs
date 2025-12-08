using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Document.DownloadDocument;

public class DownloadDocumentHandler(IRepositoryManager repositoryManager) : IRequestHandler<DownloadDocumentRequest, FileContentResult>
{
    public async Task<FileContentResult> Handle(DownloadDocumentRequest request, CancellationToken cancellationToken)
    {
        var doc = await repositoryManager.DocumentRepository
            .FindByCondition(d => d.Id == request.Id, false)
            .SingleOrDefaultAsync(cancellationToken);

        if (doc == null || doc.Content == null || doc.Content.Length == 0)
        {
            return null;
        }

        var contentType = doc.ContentType ?? "application/octet-stream";
        var fileName = string.IsNullOrWhiteSpace(doc.Name) ? $"document_{doc.Id}.{doc.Extension}" : doc.Name;
        return new FileContentResult(doc.Content, contentType) { FileDownloadName = fileName };
    }
}
