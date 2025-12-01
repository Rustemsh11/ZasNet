using System.ComponentModel;

namespace ZasNet.Domain.Enums;

public enum PaymentType
{
    [Description("Не известный тип")]
    None = 0,
    [Description("Наличная")]
    Cash = 1,
    [Description("СБП")]
    Electronic = 2,
    [Description("Карта")]
    Card = 3,
    [Description("Наличная с НДС")]
    CashWithVat = 4,
    [Description("Наличная без НДС")]
    CashWithoutVat = 5,
}
