using MediatR;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Document.AddDocument;

public class AddDocumentCommandHandler(IRepositoryManager repositoryManager) : IRequestHandler<AddDocumentCommand>
{
    public async Task Handle(AddDocumentCommand request, CancellationToken cancellationToken)
    {
        foreach (var file in request.Files)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            var fileContent = memoryStream.ToArray();

            var fileName = file.FileName;
            var extension = Path.GetExtension(fileName).TrimStart('.');

            var document = new Domain.Entities.Document
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Extension = extension,
                Content = fileContent,
                ContentType = file.ContentType,
                SizeBytes = fileContent.Length,
                OrderId = request.OrderId,
                DocumentType = request.DocumentType,
                Description = request.Description,
                UploadedUserId = request.UploadedUserId,
                UploadedDate = DateTime.UtcNow
            };

            repositoryManager.DocumentRepository.Create(document);
            await repositoryManager.SaveAsync(cancellationToken);
        }
    }
}

