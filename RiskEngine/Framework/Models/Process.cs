using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;


namespace RiskEngine.Framework
{
    public interface IProcess
    {
        string Identifier { get; }
        
        NArray Prepare(Context context);

        NArray Step(TimeInterval timeStep, NArray previous);
    }
    
    /// <summary>
    /// The Process defines how an NArray evolves across a time interval.
    /// </summary>
    public abstract class Process : Model, IProcess
    {
        public abstract NArray Prepare(Context context);

        public abstract NArray Step(TimeInterval timeStep, NArray previous);
    }
}
