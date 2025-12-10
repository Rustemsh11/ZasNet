using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Helpers;

public static class EnumsToStringConverter
{
    public static string GetOrderStatusDescription(OrderStatus orderStatus)
    {
        if (orderStatus == OrderStatus.Created)
        {
            return "Создан";
        }
        else if (orderStatus == OrderStatus.ApprovedWithEmployers)
        {
            return "Подтвержден сотрудниками";
        }
        else if (orderStatus == OrderStatus.Processing)
        {
            return "В работе";
        }
        else if (orderStatus == OrderStatus.Finished)
        {
            return "Завершен";
        }
        else if (orderStatus == OrderStatus.CreatingInvoice)
        {
            return "Ожидание счета";
        }
        else if (orderStatus == OrderStatus.AwaitingPayment)
        {
            return "Ожидание оплаты";
        }
        else if (orderStatus == OrderStatus.Closed)
        {
            return "Закрыт";
        }

        return "Не известно";
    }

    public static string GetDocumentTypeDescription(DocumentType documentType)
    {
        if (documentType == DocumentType.ActOfCompletedWorks)
        {
            return "Акт выполненных работ";
        }
        else if (documentType == DocumentType.Invoice)
        {
            return "Счет";
        }
        else if (documentType == DocumentType.WorkReport)
        {
            return "Отчет о работе";
        }

        return "Не известно";
    }
    
    public static string GetPaymentTypeDescription(PaymentType paymentType)
    {
        if (paymentType == PaymentType.Cash)
        {
            return "Наличные";
        }
        else if (paymentType == PaymentType.Electronic)
        {
            return "СБП";
        }
        else if (paymentType == PaymentType.Card)
        {
            return "Карта";
        }
        else if (paymentType == PaymentType.CashWithVat)
        {
            return "Наличные с НДС";
        }
        else if (paymentType == PaymentType.CashWithoutVat)
        {
            return "Наличные без НДС";
        }

        return "Не известно";
    }
}
