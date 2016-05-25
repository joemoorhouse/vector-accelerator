using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;

namespace RiskEngine.Factors
{   
    public class DiscountFactorNonCash : Factor<IDiscountFactor>
    {
        public NArray this[int timeIndex, DateTime t] 
        {
            get { return Model[timeIndex, t]; }
        }

        public NArray ForwardRate(int timeIndex, DateTime t1, DateTime t2)
        {
            return Model.ForwardRate(timeIndex, t1, t2);
        }
    }
}
