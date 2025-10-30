namespace ZasNet.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }

        public static User CreateUser(string login, string password, int roleId)
        {
            return new User { Login = login, Password = password, RoleId = roleId };
        }
    }
}
