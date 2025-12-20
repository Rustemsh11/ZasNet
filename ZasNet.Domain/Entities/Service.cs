namespace ZasNet.Domain.Entities
{
    public class Service : LockedItemBase
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int MeasureId { get; set; }

        public double MinVolume { get; set; }

        public decimal StandartPrecentForEmployee { get; set; }
        
        public decimal PrecentForMultipleEmployeers { get; set; }

        public decimal PrecentLaterOrderForEmployee { get; set; }

        public decimal PrecentLaterOrderForMultipleEmployeers { get; set; }

        public ICollection<OrderService> OrderServices { get; set; }

        public Measure Measure { get; set; }

    public static Service Create(
        string name, 
        decimal price, 
        int measureId, 
        double minVolume,
        decimal standartPrecentForEmployee,
        decimal precentForMultipleEmployeers,
        decimal precentLaterOrderForEmployee,
        decimal precentLaterOrderForMultipleEmployeers)
    {
        return new Service()
        {
            Name = name,
            Price = price,
            MeasureId = measureId,
            MinVolume = minVolume,
            StandartPrecentForEmployee = standartPrecentForEmployee,
            PrecentForMultipleEmployeers = precentForMultipleEmployeers,
            PrecentLaterOrderForEmployee = precentLaterOrderForEmployee,
            PrecentLaterOrderForMultipleEmployeers = precentLaterOrderForMultipleEmployeers
        };
    }

    public void UpdateService(
        string name,
        decimal price,
        int measureId,
        double minVolume,
        decimal standartPrecentForEmployee,
        decimal precentForMultipleEmployeers,
        decimal precentLaterOrderForEmployee,
        decimal precentLaterOrderForMultipleEmployeers)
    {
        this.Name = name;
        this.Price = price;
        this.MeasureId = measureId;
        this.MinVolume = minVolume;
        this.StandartPrecentForEmployee = standartPrecentForEmployee;
        this.PrecentForMultipleEmployeers = precentForMultipleEmployeers;
        this.PrecentLaterOrderForEmployee = precentLaterOrderForEmployee;
        this.PrecentLaterOrderForMultipleEmployeers = precentLaterOrderForMultipleEmployeers;
    }
}
}
