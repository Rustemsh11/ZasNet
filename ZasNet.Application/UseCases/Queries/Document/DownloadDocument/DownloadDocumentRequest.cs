using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ZasNet.Application.UseCases.Queries.Document.DownloadDocument;

public record DownloadDocumentRequest(int Id): IRequest<FileContentResult>;