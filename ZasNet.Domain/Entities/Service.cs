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

        public static Service Create(string name, decimal price, int measureId, double minValue)
        {
            return new Service()
            {
                Name = name,
                Price = price,
                MeasureId = measureId,
                MinVolume = minValue
            };
        }
    }
}
