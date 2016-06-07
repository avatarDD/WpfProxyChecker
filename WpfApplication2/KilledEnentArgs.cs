using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prxSearcher
{


    class KilledEnentArgs : EventArgs
    {
        public int mParam { get; }
        public KilledEnentArgs(int e)
        {
            mParam = e;
        }
    }
}
