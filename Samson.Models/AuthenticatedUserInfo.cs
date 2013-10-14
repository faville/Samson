using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samson.Models
{
    public class AuthenticatedUserInfo
    {
        public int UserID { get; set; }
        public int ContactID { get; set; }
        public int DomainID { get; set; }
        public Guid DomainGuid { get; set; }
        public Guid UserGuid { get; set; }
        public string DisplayName { get; set; }
        public string ContactEmail { get; set; }
        public string ExternalUrl { get; set; }
        public bool CanImpersonate { get; set; }
    }
}
