using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

/// <summary>
/// Ответ с данными заявки
/// </summary>
public class GetOrdersByFilterResponse
{
    public int Id { get; set; }

    public string Client { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string Address { get; set; }

    public decimal OrderPriceAmount { get; set; }

    public PaymentType PaymentType { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public string CreatedEmployeeName { get; set; }

    public List<string> ServiceNames { get; set; } = new();

    public List<string> CarNames { get; set; } = new();
}

