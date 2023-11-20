using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ConsoleApp1
{
    

    public partial class tblSensor
    {
        public int SensorID { get; set; }
        public Nullable<System.DateTimeOffset> DateTimeOffset { get; set; }
        public string Name { get; set; }
        public string Element { get; set; }
        public Nullable<decimal> Value { get; set; }
        public string Status { get; set; }
        public string Unit { get; set; }
        public Nullable<decimal> Minimum { get; set; }
        public Nullable<decimal> Maximum { get; set; }
        public string Fermenter { get; set; }
        public Nullable<bool> Actor { get; set; }
        public Nullable<bool> License { get; set; }
        public string Serial { get; set; }
    }
}
