using MediatR;

namespace ZasNet.Application.UseCases.Commands.Services.DeleteService;

public record DeleteServiceCommand(int Id): IRequest;

