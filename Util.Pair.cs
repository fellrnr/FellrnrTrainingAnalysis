using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Util
{
    public class Pair<A, B>
    {
        public Pair(A first, B second)
        {
            First = first;
            Second = second;
        }

        public A First { get; set; }
        public B Second { get; set; }
    }
}
