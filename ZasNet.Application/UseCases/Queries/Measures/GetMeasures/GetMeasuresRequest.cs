using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Measures.GetMeasures;

public record GetMeasuresRequest() : IRequest<List<MeasureDto>>;

