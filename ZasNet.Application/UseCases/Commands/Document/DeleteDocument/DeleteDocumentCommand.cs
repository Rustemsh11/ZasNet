using MediatR;

namespace ZasNet.Application.UseCases.Commands.Document.DeleteDocument;

public record DeleteDocumentCommand(int Id): IRequest;
