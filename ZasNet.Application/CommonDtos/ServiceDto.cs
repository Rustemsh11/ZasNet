namespace ZasNet.Application.CommonDtos;

public class ServiceDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public decimal MinPrice { get; set; }

    public double MinVolume { get; set; }

    public string Measure { get; set; }

    public decimal StandartPrecentForEmployee { get; set; }

    public decimal PrecentForMultipleEmployeers { get; set; }

    public decimal PrecentLaterOrderForEmployee { get; set; }

    public decimal PrecentLaterOrderForMultipleEmployeers { get; set; }
}
