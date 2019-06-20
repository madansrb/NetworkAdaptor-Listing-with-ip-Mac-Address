using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace assignment.Models
{
    public class CustomModel
    {
        public  List<adapterlist> adapterlists{get;set;}
        public List<ipadresslist> ipadresslists { get; set; }

        public List<macadresslist> macadresslists { get; set; }
    }

    public class adapterlist
    {
        public string adaptername { get; set; }
        
    }


    public class ipadresslist
    {
        public string ipadress { get; set; }
      
    }

    public class macadresslist
    {
        public string macadress { get; set; }
    }


    public class adapterinformation
    {
        public string networkadapter { get; set; }
        public string ipaddress { get; set; }
        public string macaddress { get; set; }
    }
}