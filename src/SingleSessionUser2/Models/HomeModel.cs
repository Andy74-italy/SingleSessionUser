using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SingleSessionUser2.Models
{
    public class HomeModel
    {
        public string SessionID { get; set; }
        public string User { get; set; }
        public string Rnd_Value_Session { get; set; }
        public string Rnd_Value_Cookie { get; set; }
    }
}