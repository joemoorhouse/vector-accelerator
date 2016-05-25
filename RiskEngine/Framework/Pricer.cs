using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;

namespace RiskEngine.Framework
{
    public interface IPricer
    {
        void Price(int timeIndex, out NArray pv);

        void Register(SimulationGraph graph);
    }
    
    public class Pricer<T> where T : Deal, new()
    {
        protected T _deal = new T();

        public T Deal { get { return _deal; } }

        public static Pricer<S> FromDeal<S>(S deal) where S :  Deal, new()
        {
            var pricer = new Pricer<S>();
            pricer._deal = deal;
            return pricer;
        }
    }

    public static class PricerExtensions
    {
        public static IEnumerable<Pricer<T>> ToPricers<T>(this IEnumerable<T> deals) where T : Deal, new()
        {
            foreach (var deal in deals) yield return Pricer<T>.FromDeal<T>(deal);
        }
    }
}
