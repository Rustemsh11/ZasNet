namespace ZasNet.Domain.Entities
{
    public class Service : LockedItemBase
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Measure { get; set; }

        public double MinVolume { get; set; }

        public ICollection<OrderService> OrderServices { get; set; }

        public static Service Create(string name, decimal price, string measure, double minValue)
        {
            return new Service()
            {
                Name = name,
                Price = price,
                Measure = measure,
                MinVolume = minValue
            };
        }
    }
}
