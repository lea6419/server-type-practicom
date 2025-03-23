public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<User> DeleteUserAsync(int userId);
    Task<User> LoginAsync(string email, string password);
    public Task<User?> GetUserByTokenAsync(string token);
    public  Task<IEnumerable<User>> GetClientAsync();

}

