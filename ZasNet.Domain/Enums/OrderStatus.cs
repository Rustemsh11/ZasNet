namespace ZasNet.Domain.Enums;

public enum OrderStatus
{
    Created = 0,
    Processing = 1,
    Finished = 2,
    CreatedInvoice = 3,
    AwaitingPayment = 3,
    Closed = 4,
}
