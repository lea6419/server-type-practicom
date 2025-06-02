using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interface.Repositories
{
    public interface IVerificationCodeRepository : IRepository<VerificationCode>
    {
        Task<VerificationCode> GetValidCodeAsync(string email, string code);
        Task RemoveExistingCodesAsync(string email);
    }

}
