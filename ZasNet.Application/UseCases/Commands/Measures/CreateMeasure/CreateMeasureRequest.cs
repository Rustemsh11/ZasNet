using MediatR;

namespace ZasNet.Application.UseCases.Commands.Measures.CreateMeasure;

public record CreateMeasureRequest(string Name) : IRequest;

