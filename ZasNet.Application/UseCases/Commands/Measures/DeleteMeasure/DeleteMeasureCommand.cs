using MediatR;

namespace ZasNet.Application.UseCases.Commands.Measures.DeleteMeasure;

public record DeleteMeasureCommand(int Id): IRequest;

