using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NArray.LinearAlgebraProviders;

namespace NArray.DeferredExecution
{
    [TestFixture]
    public class ConditionalTest
    {
        [Test]
        public void RelativeTest()
        {
            var a = new NArray(5, 1);

            using (var stream = new IntelMKLRandomNumberStream(RandomNumberGeneratorType.MRG32K3A, 111))
            {
                IntelMathKernelLibraryRandom.FillNormals(a.Storage.Data, stream);
            }

            var test1 = a < 0.1;
            var test2 = 0.1 > a;
        }
    }
}
