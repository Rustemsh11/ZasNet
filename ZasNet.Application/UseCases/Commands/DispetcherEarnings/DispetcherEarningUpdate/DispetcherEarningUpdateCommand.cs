using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Commands.DispetcherEarnings.DispetcherEarningUpdate;

public record DispetcherEarningUpdateCommand(EmployeeEarningDto EmployeeEarningDto) : IRequest;

