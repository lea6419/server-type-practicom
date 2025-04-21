using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interface.Repositories
{
    public interface IActivityLogRepository
    {
        Task<IEnumerable<ActivityLog>> GetAllAsync();
        Task AddAsync(ActivityLog log);
    }
}
