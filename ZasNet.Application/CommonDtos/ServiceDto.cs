namespace ZasNet.Application.CommonDtos;

public class ServiceDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal MinPrice { get; set; }

    public double MinVolume { get; set; }

    public string Measure { get; set; }
}
