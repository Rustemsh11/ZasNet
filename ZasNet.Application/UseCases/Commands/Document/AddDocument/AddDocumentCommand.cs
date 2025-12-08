using MediatR;
using Microsoft.AspNetCore.Http;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Commands.Document.AddDocument;

public record AddDocumentCommand(
    IEnumerable<IFormFile> Files,
    int OrderId,
    DocumentType DocumentType,
    string? Description = null,
    int? UploadedUserId = null
) : IRequest;

