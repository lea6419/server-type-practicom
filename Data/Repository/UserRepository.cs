using Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class UserRepository : Repository<User>, IUserRepository
{

    public UserRepository(ApplicationDbContext context) : base(context) { }

   

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return null;

        return user;
    }
    public async Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate)
    {
        return await _context.Users.Where(predicate).ToListAsync();
    }

}
