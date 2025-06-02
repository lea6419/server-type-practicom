using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class VerificationCode:IEntity
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string Code { get; set; }
        public DateTime Expiration { get; set; }
    }

}
