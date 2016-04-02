using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskEngine.Framework;

namespace RiskEngine.Factors
{
    public interface IMeanRevertingNormalProcess : IProcess
    {
        double Sigma { get; set; }
        double Lambda { get; set; }
    }
}
