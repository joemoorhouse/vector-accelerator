using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorAccelerator
{
    public enum CallPut { Call, Put }
    
    public class Finance
    {
        public static NArray BlackScholes(CallPut callPutFactor, NArray forward, double strike,
            NArray volatility, double deltaTime)
        {
            return BlackScholes(callPutFactor == CallPut.Call ? 1 : -1, forward, strike, volatility, deltaTime);
        }

        public static NArray BlackScholes(double callPutFactor, NArray forward, double strike,
            NArray volatility, double deltaTime)
        {
            NArray d1, d2;
            if (deltaTime == 0) return callPutFactor * (forward - strike);
            
            BlackScholesD1D2Parameters(forward, strike, volatility, deltaTime, out d1, out d2);

            var nd1 = NMath.CumulativeNormal(callPutFactor * d1);
            var nd2 = NMath.CumulativeNormal(callPutFactor * d2);

            return callPutFactor * (forward * nd1 - strike * nd2);
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
