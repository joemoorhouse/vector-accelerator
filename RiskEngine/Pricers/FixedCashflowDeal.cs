using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;
using VectorAccelerator;

namespace RiskEngine.Pricers
{
    public enum Currency { EUR, USD }
    
    class FixedCashflowDeal
    {
        public double Amount { get; set; }

        public Currency Currency { get; set; }

        public DateTime PaymentDate { get; set; }
    }

    class FixedCashflowPricer : Pricer<FixedCashflowDeal>
    {
        IDiscountFactor _df;

        public void Register(Simulation simulation)
        {
            _df = simulation.RegisterFactor<IDiscountFactor>(_deal.Currency.ToString());
        }

        public void Price(int timeIndex, out NArray pv)
        {
            pv = _deal.Amount * _df[timeIndex, _deal.PaymentDate];
        }
    }
}
