using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace VectorAccelerator
{
    public class DeferredLauncher 
    {
        dynamic _dynamicLauncher = new DynamicLauncher();

        public dynamic Launch
        {
            get { return _dynamicLauncher; }
        }
    }
    
    public class DynamicLauncher : DynamicObject
    {   
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return base.TryInvoke(binder, args, out result);
        }

        public override bool TrySetMember(
        SetMemberBinder binder, object value)
        {
            return true;
        }

        public override bool TryGetMember(
        GetMemberBinder binder, out object result)
        {
            result = null;
            return true;
        }
    }
}
