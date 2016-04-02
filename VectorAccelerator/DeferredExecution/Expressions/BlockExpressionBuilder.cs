using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class BlockExpressionBuilder
    {
        List<VectorParameterExpression> _localParameters = new List<VectorParameterExpression>();
        Dictionary<INArray, VectorParameterExpression> _argumentLookup = new Dictionary<INArray, VectorParameterExpression>(); 
        List<BinaryExpression> _operations = new List<BinaryExpression>();

        public VectorBlockExpression ToBlock()
        {
            return new VectorBlockExpression()
            {
                Operations = _operations, 
                ArgumentParameters = _argumentLookup.Values.ToArray(), 
                LocalParameters = _localParameters };
        }

        public void AddAssign<T>(NArray<T> result, NArray<T> a)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result), GetParameter<T>(a)));
        }

        public void AddBinaryElementWiseOperation<T>(NArray<T> a, NArray<T> b,
            NArray<T> result, ExpressionType operation)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                Expression.MakeBinary(operation, GetParameter<T>(a), GetParameter<T>(b)))
                );
        }

        public void AddUnaryElementWiseOperation<T>(NArray<T> a,
            NArray<T> result, UnaryElementWiseOperation operation)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                ExpressionExtended.MakeUnary(operation, GetParameter<T>(a)))
                );
        }

        public void AddScaleOffsetOperation<T>(NArray<T> a, T scale, T offset, NArray<T> result)
        {
            _operations.Add(
                Expression.Assign(GetParameter<T>(result),
                ExpressionExtended.ScaleOffset<T>(GetParameter<T>(a), scale, offset))
                );
        }

        public ILocalNArray CreateLocalOfLength<T>(int length)
        {
            //ILocalNArray localArray = null;
            NArray<T> localArray = null;
            if (typeof(T) == typeof(double)) localArray = new LocalNArray(_localParameters.Count, length) as NArray<T>;
            else if (typeof(T) == typeof(int)) localArray = new LocalNArrayInt(_localParameters.Count, length) as NArray<T>;
            var parameter = new LocalNArrayVectorParameterExpression<T>(localArray);
            _localParameters.Add(parameter);
            return localArray as ILocalNArray;
        }

        private VectorParameterExpression GetParameter<T>(NArray<T> array)
        {
            var local = array as ILocalNArray;
            if (local != null)
            {
                return _localParameters[local.Index] as VectorParameterExpression;
            }
            else
            {
                VectorParameterExpression argument;
                // is this fast enough?
                if (!_argumentLookup.TryGetValue(array, out argument))
                {
                    argument = new ReferencingVectorParameterExpression<T>(array, ParameterType.Argument, _argumentLookup.Count);
                    _argumentLookup.Add(array, argument);
                }
                return argument;
            }
        }
    }
}
