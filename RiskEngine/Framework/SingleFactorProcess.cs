using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator;


namespace RiskEngine.Framework
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SingleFactorProcess : Model
    {
        public NArray Value { get; protected set; }

        internal abstract void Prepare(Context context);

        internal override void StepNext()
        {
            var timeStep = _timeIntervals[_currentTimeIndex];
            Value = Step(timeStep, Value);
            _currentTimeIndex++;
        }

        protected abstract NArray Step(TimeInterval timeStep, NArray previous);
    }
}
