using System.ComponentModel;

namespace ZasNet.Domain.Enums;

public enum OrderStatus
{
    [Description("Создан")]
    Created = 0,
    [Description("Подтвержден сотрудниками")]
    ApprovedWithEmployers = 1,
    [Description("В работе")]
    Processing = 2,
    [Description("Завершен")]
    Finished = 3,
    [Description("Ожидание счета")]
    CreatingInvoice = 4,
    [Description("Ожидание оплаты")]
    AwaitingPayment = 5,
    [Description("Закрыт")]
    Closed = 6,
}
