using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Document.ViewDocument;

public class ViewDocumentHandler(IRepositoryManager repositoryManager) : IRequestHandler<ViewDocumentRequest, FileContentResult>
{
    public async Task<FileContentResult> Handle(ViewDocumentRequest request, CancellationToken cancellationToken)
    {
        var doc = await repositoryManager.DocumentRepository
           .FindByCondition(d => d.Id == request.Id, false)
           .SingleOrDefaultAsync(cancellationToken);

        if (doc == null || doc.Content == null || doc.Content.Length == 0)
        {
            return null;
        }

        var contentType = doc.ContentType ?? "application/octet-stream";
        return new FileContentResult(doc.Content, contentType);
    }
}
