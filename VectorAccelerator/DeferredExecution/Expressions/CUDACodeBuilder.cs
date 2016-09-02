using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorAccelerator.DeferredExecution.Expressions;
//using System.Linq.Expressions;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    public class CUDACodeBuilder
    {
        StringBuilder _code = new StringBuilder();
        StringBuilder _bodySection = new StringBuilder();

        public enum ParameterType { Vector, VectorList };

        /// <summary>
        /// Template for a vector operation that may be split across
        /// threads in a block and between blocks
        /// </summary>
        const string prototype_vectorOperation = @"
            extern ""C"" __global__ 
                void ___functionName(___parameters)
                {
                    size_t tid = blockIdx.x * blockDim.x + threadIdx.x; 
                    ___localsDeclaration
                    if (tid < n) 
                    {
                        ___body
                    }
                 }
            ";

        const string indexing = "[tid]";

        string functionName = "___functionName";

        string parameterSection = "___parameters";

        string localsDeclarationSection = "___localsDeclaration";

        string bodySection = "___body";

        public void GenerateCode(VectorBlockExpression block)
        {
            Visit(block, _bodySection);
            
            _code.Replace(functionName, "TestFunction");
            ReplaceWithIndentation(_code, bodySection, _bodySection);

            var code = _code.ToString();
        }

        public string GetCode()
        {
            return _code.ToString();
        }

        public void SetKernelArguments(IEnumerable<string> parameters)
        {
            _code.Replace(parameterSection,
                CommaSeparated(
                    parameters.Select(p => VectorType + Space + p)
                        ));
        }

        public void SetKernelLocals(IEnumerable<string> locals)
        {
            _code.Replace(localsDeclarationSection,
                SemiColonSeparated(
                    locals.Select(l => VectorElementType + Space + l)
                        ) + ";");
        }

        public static void ReplaceWithIndentation(StringBuilder stringBuilder, string oldValue, StringBuilder newValue)
        {
            string indentation = GetIndentation(stringBuilder, oldValue);
            TrimLastLineEnd(newValue);
            newValue.Replace(Environment.NewLine, Environment.NewLine + indentation);
            stringBuilder.Replace(oldValue, newValue.ToString());
        }

        public static string GetIndentation(StringBuilder builder, string value)
        {
            int lastEndOfLineIndex = 0;
            int length = value.Length;
            for (int i = 0; i < builder.Length; ++i)
            {
                if (builder[i] == '\n' || builder[i] == '\r')
                {
                    lastEndOfLineIndex = i;
                }
                if (builder[i] == value[0])
                {
                    int num = 1;
                    while ((num < length) && (builder[i + num] == value[num]))
                    {
                        num++;
                    }
                    if (num == length)
                    {
                        return builder.ToString(lastEndOfLineIndex + 1, i - lastEndOfLineIndex - 1);
                    }
                }
            }
            return String.Empty;
        }

        public static void TrimLastLineEnd(StringBuilder builder)
        {
            if (builder[builder.Length - 2] == '\r' && builder[builder.Length - 1] == '\n')
            {
                builder.Remove(builder.Length - 2, 2);
            }
        }

        public void WriteEquals(StringBuilder builder)
        {
            builder.Append(" = ");
        }

        public string Space
        {
            get { return " "; }
        }

        public string VectorType
        {
            get { return "float*"; }
        }

        public string VectorElementType
        {
            get { return "float"; }
        }

        public static string CommaSeparated(IEnumerable<string> items)
        {
            return DelimiterSeparated(items, ',');
        }

        public static string SemiColonSeparated(IEnumerable<string> items)
        {
            return DelimiterSeparated(items, ';');
        }

        public static string DelimiterSeparated(IEnumerable<string> items, char delimiter)
        {
            string value = items.First();
            return items
                .Skip(1)
                    .Aggregate(value, (p, n) => p + delimiter + " " + n);
        }

        public static void Write(string code, StringBuilder builder)
        {
            builder.Append(code);
        }

        public static void WriteType(Type type, StringBuilder builder)
        {
            builder.Append(type.Name);
        }

        public static void WriteExpression(Expression expression, StringBuilder builder)
        {
            builder.Append(expression.ToString());
        }

        public static void WriteSemiColon(StringBuilder builder)
        {
            builder.Append(';');
        }

        public static void WriteNewLine(StringBuilder builder)
        {
            builder.Append(Environment.NewLine);
        }

        public Expression Visit(Expression exp, StringBuilder builder)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Block:
                    return VisitBlock((VectorBlockExpression)exp, builder);
                case ExpressionType.Assign:
                    return VisitAssign((BinaryExpression)exp, builder);

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp, builder);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp, builder);
                case ExpressionType.TypeIs:
                case ExpressionType.Conditional:
                //    return VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    //return VisitConstant((ConstantExpression)exp, builder);
                case ExpressionType.Parameter:
                    return VisitParameter((VectorParameterExpression)exp, builder);
                case ExpressionType.MemberAccess:
                case ExpressionType.Call:
                    return VisitMethodCall((UnaryMathsExpression)exp, builder);
                case ExpressionType.Lambda:
                case ExpressionType.New:
                //    return VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                //    return VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                //    return VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                //    return VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                //    return VisitListInit((ListInitExpression)exp);
                default:
                    throw new NotSupportedException(string.Format("Expressions of type {0} are not supported", exp.NodeType));
            }
        }

        private Expression VisitBlock(VectorBlockExpression block, StringBuilder builder)
        {   
            _code.Append(prototype_vectorOperation);

            var arguments = block.ArgumentParameters;

            var locals = block.LocalParameters;

            //builder.
            SetKernelArguments(arguments.Select(a => a.Name));
            SetKernelLocals(locals.Select(a => a.Name));

            foreach (var expression in block.Operations)
            {
                Visit(expression, builder);
                WriteNewLine(builder);
            }
            return block;
        }

        //private bool VariableIsParameter(ParameterExpression variable)
        //{
        //    return variable.Name.StartsWith("parameter"); // as opposed to "local"
        //}

        protected Expression VisitBinary(BinaryExpression binary, StringBuilder builder)
        {
            Visit(binary.Left, builder);
            Write(" ", builder);
            Write(Operator(binary.NodeType), builder);
            Write(" ", builder);
            Visit(binary.Right, builder);
            return binary;
        }

        private string Operator(ExpressionType expression)
        {
            switch (expression)
            {
                case (ExpressionType.Add):
                    return "+";
                case (ExpressionType.Subtract):
                    return "-";
                case (ExpressionType.Multiply):
                    return "+";
                case (ExpressionType.Divide):
                    return "-";
                default:
                    return "<unknown operator>";
            }
        }

        protected Expression VisitUnary(UnaryExpression unary, StringBuilder builder)
        {
            return unary;
        }

        protected Expression VisitMethodCall(UnaryMathsExpression call, StringBuilder builder)
        {
            string function = string.Empty;
            switch (call.UnaryType)
            {
                case UnaryElementWiseOperation.Exp:
                    function = "exp";
                    break;
                case UnaryElementWiseOperation.ScaleOffset:
                    // special case:
                    if (call is ScaleOffsetExpression<double>)
                    {
                        var expression = call as ScaleOffsetExpression<double>;
                        Write(expression.Offset + " + " + expression.Scale + " * ", builder);
                        Visit(call.Operand, builder);
                        return call;
                    }
                    break;
                default:
                    function = "n/a";
                    break;
            }
            Write(function, builder);
            Write("(", builder);
            Visit(call.Operand, builder);  
            Write(")", builder);
            return call;
        }

        protected Expression VisitParameter(VectorParameterExpression variable, StringBuilder builder)
        {
            Write(variable.Name, builder);
            if (variable.ParameterType == Expressions.ParameterType.Argument)
            {
                Write(indexing, builder);
            }
            return variable;
        }

        //protected Expression VisitConstant(ConstantExpression variable, StringBuilder builder)
        //{
        //    Write(variable.Value.ToString(), builder);
        //    return variable;
        //}

        private Expression VisitAssign(BinaryExpression binary, StringBuilder builder)
        {
            Visit(binary.Left, builder);
            WriteEquals(builder);
            Visit(binary.Right, builder);
            WriteSemiColon(builder);
            return binary;
        }
    }
}
