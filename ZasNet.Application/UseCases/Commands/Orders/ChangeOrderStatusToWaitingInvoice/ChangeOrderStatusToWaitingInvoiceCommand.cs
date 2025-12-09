using MediatR;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatusToWaitingInvoice;

public record ChangeOrderStatusToWaitingInvoiceCommand(int Id, bool isNeedInvoiceArgently) : IRequest;
