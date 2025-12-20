using MediatR;

namespace ZasNet.Application.UseCases.Commands.Measures.UpdateMeasure;

public record UpdateMeasureCommand(int Id, string Name) : IRequest;

