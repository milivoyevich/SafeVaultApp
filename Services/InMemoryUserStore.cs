public class InMemoryUserStore
{
    private static readonly List<User> _users = new()
    {
        new User { Username = "admin", Password = "SecurePass123!" },
        new User { Username = "vaultuser", Password = "Vault123!" }
    };

    public User ValidateUser(string username, string password)
        => _users.SingleOrDefault(u => u.Username == username && u.Password == password);
}