using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using RiskEngine.Factors;
using VectorAccelerator;

namespace RiskEngine.Pricers
{
    public class FloatingCashflowDeal : Deal
    {
        public double Amount { get; set; }

        public Currency Currency { get; set; }

        public DateTime PaymentDate { get; set; }
    }

    public class FloatingCashflowPricer : Pricer<FloatingCashflowDeal>
    {
        DiscountFactorNonCash _df;

        public FloatingCashflowPricer(FloatingCashflowDeal deal)
        {
            _deal = deal;
        }

        public void Register(SimulationGraph graph)
        {
            _df = graph.RegisterFactor<DiscountFactorNonCash>(_deal.Currency.ToString());
        }

        public void Price(int timeIndex, out NArray pv)
        {
            pv = _deal.Amount * _df[timeIndex, _deal.PaymentDate];
        }
    }
}
