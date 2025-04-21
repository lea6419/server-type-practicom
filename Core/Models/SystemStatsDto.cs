using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SystemStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalFiles { get; set; }
        public int FilesWaiting { get; set; }
        public int FilesInProgress { get; set; }
        public int FilesCompleted { get; set; }
        public int TypistsCount { get; set; }
        public int ClientsCount { get; set; }
    }
}
