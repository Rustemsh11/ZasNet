using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Entities;

public class Order : LockedItemBase
{
    public string Client {  get; set; }

    public DateTime Date { get; set; }

    public string AddressCity { get; set; }

    public string AddressStreet { get; set; }

    public string AddressNumber { get; set; }

    public decimal OrderPriceAmount { get; set; }

    public PaymentType PaymentType { get; set; }

    public string? Description { get; set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime? ClosedDate { get; private set;}

    public ClientType? ClientType { get; private set;}

    public int CreatedEmployeeId { get; set; }

    public ICollection<OrderService> OrderServices { get; set; }

    public ICollection<OrderCar> OrderCars { get; set; }

    public ICollection<OrderEmployee> OrderEmployees { get; set; }

    public ICollection<Document> OrderDocuments { get; set; }

    public Employee CreatedEmployee { get; set; }

    public class CreateOrderDto
    {
        public string Client { get; set; }

        public DateTime Date { get; set; }

        public string AddressCity { get; set; }

        public string AddressStreet { get; set; }

        public string AddressNumber { get; set; }

        public decimal OrderPriceAmount { get; set; }

        public PaymentType PaymentType { get; set; }

        public string? Description { get; set; }

        public int CreatedEmployeeId { get; set; }

        public ClientType? ClientType { get; set; }

        public List<OrderService> OrderServices { get; set; }

        public List<OrderCar> OrderCars { get; set; }

        public List<OrderEmployee> OrderEmployees { get; set; }
    }
    
    public static Order Create(CreateOrderDto orderDto)
    {
        return new Order()
        {
            Client = orderDto.Client,
            Date = orderDto.Date,
            AddressCity = orderDto.AddressCity,
            AddressStreet = orderDto.AddressStreet,
            AddressNumber = orderDto.AddressNumber,
            OrderPriceAmount = orderDto.OrderPriceAmount,
            PaymentType = orderDto.PaymentType,
            Description = orderDto.Description,
            Status = OrderStatus.Created,
            CreatedDate = DateTime.Now,
            OrderServices = orderDto.OrderServices,
            OrderCars = orderDto.OrderCars,
            OrderEmployees = orderDto.OrderEmployees,
            ClientType = orderDto.ClientType,
            CreatedEmployeeId = orderDto.CreatedEmployeeId,
        };
    }
}
