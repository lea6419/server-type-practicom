using System.Linq.Expressions;

public interface IUserRepository : IRepository<User>
{

    Task<User> GetByUsernameAsync(string username);
    Task<User> LoginAsync(string email, string password);

    Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate);
}
