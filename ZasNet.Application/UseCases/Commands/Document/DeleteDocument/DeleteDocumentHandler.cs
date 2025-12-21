using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Document.DeleteDocument;

public class DeleteDocumentHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteDocumentCommand>
{
    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await repositoryManager.DocumentRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);

        if (doc == null)
        {
            throw new ArgumentException($"Документ с id {request.Id} не найден");
        }

        repositoryManager.DocumentRepository.Delete(doc);
        
        await repositoryManager.SaveAsync(cancellationToken);
    }
}
