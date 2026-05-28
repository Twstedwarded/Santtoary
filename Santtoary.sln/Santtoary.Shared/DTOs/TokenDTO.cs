using System;
using System.Collections.Generic;
using System.Text;

namespace Santtoary.Shared.DTOs
{
    public class TokenDTO 
    {
        public string Token { get; set; }= null!;
        public DateTime Expiration { get; set; }
    }
}
