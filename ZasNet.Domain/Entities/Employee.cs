namespace ZasNet.Domain.Entities;

public class Employee : LockedItemBase
{
    public string Name { get; set; }

    public string? Phone { get; set; }

    public long? ChatId {  get; set; }

    public ICollection<OrderServiceEmployee> OrderServiceEmployees { get; set; }

    public ICollection<Order> CreatedByEmployeeOrder { get; set; }


    public void SetChatId(long chatId)
    {
        this.ChatId = chatId;
    }
}
