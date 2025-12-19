using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Commands.EmployeeEarnings.EmployeeEarningUpdate;

public record EmployeeEarningUpdateCommand(EmployeeEarningDto EmployeeEarningDto) : IRequest;
