using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prxSearcher
{


    class KilledEnentArgs : EventArgs
    {
        private int _mParam { get; set; }
        public int mParam
        {
            get
            {
                return _mParam;
            }
        }
        public KilledEnentArgs(int e)
        {
            _mParam = e;
        }
    }
}
