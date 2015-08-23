using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAcceleration
{
    public class Examples
    {
        public static void Run()
        {
            NCube a = new NCube();
            NCube b = new NCube();
            NCube c = new NCube();
            
            NCube result = new NCube();

            a.AssignIfElse(a,
                a + b,
                a + c);

            // need to keep track of 
            //
            // for if else, need to work back up the stack and identify sections that apply to 

            a.AssignIfElse2(a,
                () => { return a + b; }, 
                () => { return a + c; });

            BasicExample(a, b, c, 3, result);

            ExecutionContext.ExecuteDeferred(() => BasicExample(a, b, c, 3, result));
            

            //var launcher = new DeferredLauncher();
            //launcher.Launch.BasicExample(a, b, c, 3, result);
            
        }

        public void HJMModelStepExample()
        {
            //NCube 
        }


        public static void Test(Delegate general)
        {
        }

        public static void BasicExample(NCube a, NCube b, NCube c, double d, NCube result)
        {
            // what if the NCube is a matrix?
            // a.MatrixMultiply(b)? 
            // if Matrix-Vector, then makes sense to include in parallelize as long as matrix dimension is small
            result.Assign(c * a * b + d);
        }
    }
}
