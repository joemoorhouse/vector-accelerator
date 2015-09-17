using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public class Finance
    {
        public static NArray BlackScholes(double putCallFactor, NArray forward, double strike,
            NArray volatility, double deltaTime)
        {
            NArray d1, d2;
            if (deltaTime == 0) return putCallFactor * (forward - strike);
            
            BlackScholesD1D2Parameters(forward, strike, volatility, deltaTime, out d1, out d2);
            
            var nd1 = NMath.CumulativeNormal(putCallFactor * d1);
            var nd2 = NMath.CumulativeNormal(putCallFactor * d2);
            
            return putCallFactor * (forward * nd1 - strike * nd2);
        }

        public static void BlackScholesD1D2Parameters(NArray forward, double strike, NArray volatility,
            double deltaTime, out NArray d1, out NArray d2)
        {
            var volMultSqrtDTime = volatility * Math.Sqrt(deltaTime);

            d1 = (NMath.Log(forward / strike) + (0.5 * volatility * volatility * deltaTime))
                / volMultSqrtDTime;

            d2 = d1 - volMultSqrtDTime;
        }
    }
}
