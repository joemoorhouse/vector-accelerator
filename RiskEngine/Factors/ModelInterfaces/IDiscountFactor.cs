using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;
using RiskEngine.Framework;

namespace RiskEngine
{
    /// <summary>
    /// Interface to be implemented by a DiscountFactor Model.
    /// </summary>
    public interface IDiscountFactor
    {
        /// <summary>
        /// Returns discount factor (zero coupon bond price)
        /// </summary>
        /// <param name="timePoint">Simulation time index</param>
        /// <param name="t">Zero coupon bond maturity</param>
        /// <returns></returns>
        NArray this[int timeIndex, DateTime t] { get; }
    }
}
