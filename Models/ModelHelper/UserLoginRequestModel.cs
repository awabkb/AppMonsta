using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMK_web.Models.ModelHelper
{
    public class UserLoginRequestModel
    {
        public string NameOrMail { get; set; }
        public string Password { get; set; }
    }
}
