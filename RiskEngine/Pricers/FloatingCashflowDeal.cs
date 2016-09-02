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
        public double Notional { get; set; }

        public Currency Currency { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class FloatingCashflowPricer : Pricer<FloatingCashflowDeal>, IPricer
    {
        DiscountFactorNonCash _df;
        int _fixingTimeIndex;

        public FloatingCashflowPricer(FloatingCashflowDeal deal)
        {
            _deal = deal;
        }

        public override void Register(SimulationGraph graph)
        {
            base.Register(graph);
            _df = graph.RegisterFactor<DiscountFactorNonCash>(_deal.Currency.ToString());
        }

        public void PrePrice()
        {
            _fixingTimeIndex = Array.BinarySearch(_timePoints, _deal.StartDate);
            if (_fixingTimeIndex < 0)
            {
                _fixingTimeIndex = -_fixingTimeIndex;
            }
        }

        public DateTime ExposureEndDate { get { return _deal.EndDate; } }

        public void Price(int timeIndex, out NArray pv)
        {
            if (_timePoints[timeIndex] > _deal.EndDate)
            {
                pv = 0; return;
            }
            var forwardRate = (_timePoints[timeIndex] > _deal.StartDate)
                ? _df.ForwardRate(_fixingTimeIndex, _deal.StartDate, Deal.EndDate)
                : _df.ForwardRate(timeIndex, _deal.StartDate, Deal.EndDate);

            var coverage = (_deal.EndDate - _deal.StartDate).TotalDays / 365.25;
            pv = _deal.Notional * coverage * forwardRate * _df[timeIndex, _deal.EndDate];    
        }
    }
}
