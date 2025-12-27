namespace ZasNet.Domain.Enums;

public enum OrderStatus
{
    Created = 0,
    ApprovedWithEmployers = 1,
    Processing = 2,
    Finished = 3,
    CreatingInvoice = 4,
    SendingPayment = 5,
    AwaitingPayment = 6,
    Closed = 7,
}
