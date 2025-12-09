using ZasNet.Domain.Enums;

namespace ZasNet.Domain.Entities;

    public class Order : LockedItemBase
{
    public string Client {  get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }

    public string AddressCity { get; set; }

    public string AddressStreet { get; set; }

    public string AddressNumber { get; set; }

    public decimal OrderPriceAmount { get; set; }

    public PaymentType PaymentType { get; set; }

    public string? Description { get; set; }

    public OrderStatus Status { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime? ClosedDate { get; private set;}

    public int CreatedEmployeeId { get; set; }

    public bool? NeedInvoiceUrgently { get; set; }
    
    public ICollection<OrderService> OrderServices { get; set; }

    public ICollection<Document> OrderDocuments { get; set; }

    public Employee CreatedEmployee { get; set; }

	public int? FinishedEmployeeId { get; set; }

	public Employee? FinishedEmployee { get; set; }

    public class UpsertOrderDto
    {
        public string Client { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        public string AddressCity { get; set; }

        public string AddressStreet { get; set; }

        public string AddressNumber { get; set; }

        public decimal OrderPriceAmount { get; set; }

        public PaymentType PaymentType { get; set; }

        public OrderStatus Status { get; set; }

        public string? Description { get; set; }

        public int CreatedEmployeeId { get; set; }

        public List<OrderService> OrderServices { get; set; }
    }
    
    public static Order Create(UpsertOrderDto orderDto)
    {
        return new Order()
        {
            Client = orderDto.Client,
            DateStart = orderDto.DateStart,
            DateEnd = orderDto.DateEnd,
            AddressCity = orderDto.AddressCity,
            AddressStreet = orderDto.AddressStreet,
            AddressNumber = orderDto.AddressNumber,
            OrderPriceAmount = orderDto.OrderPriceAmount,
            PaymentType = orderDto.PaymentType,
            Description = orderDto.Description,
            Status = orderDto.Status,
            CreatedDate = DateTime.Now,
            OrderServices = orderDto.OrderServices,
            CreatedEmployeeId = orderDto.CreatedEmployeeId,
        };
    }

    public void Update(UpsertOrderDto orderDto) 
    {
        Client = orderDto.Client;
        DateStart = orderDto.DateStart;
        DateEnd = orderDto.DateEnd;
        AddressCity = orderDto.AddressCity;
        AddressStreet = orderDto.AddressStreet;
        AddressNumber = orderDto.AddressNumber;
        OrderPriceAmount = orderDto.OrderPriceAmount;
        PaymentType = orderDto.PaymentType;
        Description = orderDto.Description;
        Status = orderDto.Status;
        OrderServices = orderDto.OrderServices;
        CreatedEmployeeId = orderDto.CreatedEmployeeId;
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        ClosedDate = status == OrderStatus.Closed ? DateTime.Now : null;
    }
}
