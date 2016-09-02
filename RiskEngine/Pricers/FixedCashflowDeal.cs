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
    public enum Currency { EUR, USD }
    
    public class FixedCashflowDeal : Deal
    {
        public double Notional { get; set; }

        public Currency Currency { get; set; }

        public double Rate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class FixedCashflowPricer : Pricer<FixedCashflowDeal>, IPricer
    {
        DiscountFactorNonCash _df;

        public FixedCashflowPricer(FixedCashflowDeal deal)
        {
            _deal = deal;
        }

        public override void Register(SimulationGraph graph)
        {
            base.Register(graph);
            _df = graph.RegisterFactor<DiscountFactorNonCash>(_deal.Currency.ToString());
        }

        public void PrePrice() { }

        public DateTime ExposureEndDate { get { return _deal.EndDate; } }

        public void Price(int timeIndex, out NArray pv)
        {
            if (_timePoints[timeIndex] > _deal.EndDate)
            {
                pv = 0; return;
            }
            var coverage = (_deal.EndDate - _deal.StartDate).TotalDays / 365.25;
            pv = _deal.Notional * _deal.Rate * coverage * _df[timeIndex, _deal.EndDate];
        }
    }
}
