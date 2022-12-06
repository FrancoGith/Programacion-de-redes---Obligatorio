using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Log
    {
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }

        public Log()
        { 
            Date = DateTime.Today;
        }
    }
}
