namespace ZasNet.Domain.Entities;

public class Employee : LockedItemBase
{
    public string Name { get; set; }

    public string? Phone { get; set; }

    public long? ChatId {  get; set; }

    public string Login { get; set; }

    public string Password { get; set; }

    public int RoleId { get; set; }

    public Role Role { get; set; }

    public ICollection<OrderServiceEmployee> OrderServiceEmployees { get; set; }

    public ICollection<Order> CreatedByEmployeeOrder { get; set; }

    public static Employee CreateEmployee(string name, string? phone, string login, string password, Role role)
    {
        return new Employee { Name = name, Phone = phone, Login = login, Password = password, RoleId = role.Id };
    }

    public void SetChatId(long chatId)
    {
        this.ChatId = chatId;
    }
}
