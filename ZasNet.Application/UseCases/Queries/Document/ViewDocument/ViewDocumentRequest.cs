using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ZasNet.Application.UseCases.Queries.Document.ViewDocument;

public record ViewDocumentRequest(int Id) : IRequest<FileContentResult>;
