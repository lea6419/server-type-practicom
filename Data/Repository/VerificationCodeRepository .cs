using Core.Interface.Repositories;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Data.Repository
{
    public class VerificationCodeRepository : Repository<VerificationCode>, IVerificationCodeRepository
    {
        public VerificationCodeRepository(ApplicationDbContext context ) : base(context) { }

        public async Task<VerificationCode> GetValidCodeAsync(string email, string code)
        {
            return await _context.VerificationCodes
                .FirstOrDefaultAsync(vc => vc.UserEmail == email && vc.Code == code && vc.Expiration > DateTime.UtcNow);
        }

        public async Task RemoveExistingCodesAsync(string email)
        {
            var codes = await _context.VerificationCodes.Where(vc => vc.UserEmail == email).ToListAsync();
            _context.VerificationCodes.RemoveRange(codes);
            await _context.SaveChangesAsync();
        }
    }

}
