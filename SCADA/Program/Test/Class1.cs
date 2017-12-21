using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
 public   class Class1
    {
        private long k = 19;
        public static int hi { get; set; }

        public int kl { get; private set; }

        static Class1()
        {
            hi = 10;
            
        }
    }
}
