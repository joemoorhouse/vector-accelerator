﻿using System;
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

        public void Register(SimulationGraph graph)
        {
            _df = graph.RegisterFactor<DiscountFactorNonCash>(_deal.Currency.ToString());
        }

        public void Price(int timeIndex, out NArray pv)
        {
            var coverage = (_deal.EndDate - _deal.StartDate).Days / 365.35;
            pv = _deal.Notional * _deal.Rate * coverage * _df[timeIndex, _deal.EndDate];
        }
    }
}
