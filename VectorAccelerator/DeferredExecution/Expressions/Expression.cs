using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorAccelerator.DeferredExecution.Expressions
{
    /// <summary>
    /// A light-weight version of Expression optimised for the needs of VecorAccelerator.
    /// </summary>
    public abstract class Expression
    {        
        public abstract ExpressionType NodeType { get; }

        public abstract Type Type { get; }

        public static BinaryExpression Add(Expression left, Expression right)
        {
            CheckTypes(left, right);
            return new SimpleBinaryExpression(ExpressionType.Add, left, right, left.Type);
        }

        public static BinaryExpression Assign(Expression left, Expression right)
        {
            CheckTypes(left, right);
            return new AssignBinaryExpression(left, right);
        }

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            CheckTypes(left, right);
            return new SimpleBinaryExpression(ExpressionType.Divide, left, right, left.Type);
        }

        public static BinaryExpression Multiply(Expression left, Expression right)
        {
            CheckTypes(left, right);
            return new SimpleBinaryExpression(ExpressionType.Multiply, left, right, left.Type);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
        {
            CheckTypes(left, right);
            return new SimpleBinaryExpression(binaryType, left, right, left.Type);
        }

        private static void CheckTypes(Expression left, Expression right)
        {
            if (left.Type != right.Type) throw new ArgumentException("expressions Types mismatch");
        }
    }

        // Summary:
    //     Describes the node types for the nodes of an expression tree.
    public enum ExpressionType
    {
        // Summary:
        //     An addition operation, such as a + b, without overflow checking, for numeric
        //     operands.
        Add = 0,
        //
        // Summary:
        //     An addition operation, such as (a + b), with overflow checking, for numeric
        //     operands.
        AddChecked = 1,
        //
        // Summary:
        //     A bitwise or logical AND operation, such as (a & b) in C# and (a And b) in
        //     Visual Basic.
        And = 2,
        //
        // Summary:
        //     A conditional AND operation that evaluates the second operand only if the
        //     first operand evaluates to true. It corresponds to (a && b) in C# and (a
        //     AndAlso b) in Visual Basic.
        AndAlso = 3,
        //
        // Summary:
        //     An operation that obtains the length of a one-dimensional array, such as
        //     array.Length.
        ArrayLength = 4,
        //
        // Summary:
        //     An indexing operation in a one-dimensional array, such as array[index] in
        //     C# or array(index) in Visual Basic.
        ArrayIndex = 5,
        //
        // Summary:
        //     A method call, such as in the obj.sampleMethod() expression.
        Call = 6,
        //
        // Summary:
        //     A node that represents a null coalescing operation, such as (a ?? b) in C#
        //     or If(a, b) in Visual Basic.
        Coalesce = 7,
        //
        // Summary:
        //     A conditional operation, such as a > b ? a : b in C# or If(a > b, a, b) in
        //     Visual Basic.
        Conditional = 8,
        //
        // Summary:
        //     A constant value.
        Constant = 9,
        //
        // Summary:
        //     A cast or conversion operation, such as (SampleType)obj in C#or CType(obj,
        //     SampleType) in Visual Basic. For a numeric conversion, if the converted value
        //     is too large for the destination type, no exception is thrown.
        Convert = 10,
        //
        // Summary:
        //     A cast or conversion operation, such as (SampleType)obj in C#or CType(obj,
        //     SampleType) in Visual Basic. For a numeric conversion, if the converted value
        //     does not fit the destination type, an exception is thrown.
        ConvertChecked = 11,
        //
        // Summary:
        //     A division operation, such as (a / b), for numeric operands.
        Divide = 12,
        //
        // Summary:
        //     A node that represents an equality comparison, such as (a == b) in C# or
        //     (a = b) in Visual Basic.
        Equal = 13,
        //
        // Summary:
        //     A bitwise or logical XOR operation, such as (a ^ b) in C# or (a Xor b) in
        //     Visual Basic.
        ExclusiveOr = 14,
        //
        // Summary:
        //     A "greater than" comparison, such as (a > b).
        GreaterThan = 15,
        //
        // Summary:
        //     A "greater than or equal to" comparison, such as (a >= b).
        GreaterThanOrEqual = 16,
        //
        // Summary:
        //     An operation that invokes a delegate or lambda expression, such as sampleDelegate.Invoke().
        Invoke = 17,
        //
        // Summary:
        //     A lambda expression, such as a => a + a in C# or Function(a) a + a in Visual
        //     Basic.
        Lambda = 18,
        //
        // Summary:
        //     A bitwise left-shift operation, such as (a << b).
        LeftShift = 19,
        //
        // Summary:
        //     A "less than" comparison, such as (a < b).
        LessThan = 20,
        //
        // Summary:
        //     A "less than or equal to" comparison, such as (a <= b).
        LessThanOrEqual = 21,
        //
        // Summary:
        //     An operation that creates a new System.Collections.IEnumerable object and
        //     initializes it from a list of elements, such as new List<SampleType>(){ a,
        //     b, c } in C# or Dim sampleList = { a, b, c } in Visual Basic.
        ListInit = 22,
        //
        // Summary:
        //     An operation that reads from a field or property, such as obj.SampleProperty.
        MemberAccess = 23,
        //
        // Summary:
        //     An operation that creates a new object and initializes one or more of its
        //     members, such as new Point { X = 1, Y = 2 } in C# or New Point With {.X =
        //     1, .Y = 2} in Visual Basic.
        MemberInit = 24,
        //
        // Summary:
        //     An arithmetic remainder operation, such as (a % b) in C# or (a Mod b) in
        //     Visual Basic.
        Modulo = 25,
        //
        // Summary:
        //     A multiplication operation, such as (a * b), without overflow checking, for
        //     numeric operands.
        Multiply = 26,
        //
        // Summary:
        //     An multiplication operation, such as (a * b), that has overflow checking,
        //     for numeric operands.
        MultiplyChecked = 27,
        //
        // Summary:
        //     An arithmetic negation operation, such as (-a). The object a should not be
        //     modified in place.
        Negate = 28,
        //
        // Summary:
        //     A unary plus operation, such as (+a). The result of a predefined unary plus
        //     operation is the value of the operand, but user-defined implementations might
        //     have unusual results.
        UnaryPlus = 29,
        //
        // Summary:
        //     An arithmetic negation operation, such as (-a), that has overflow checking.
        //     The object a should not be modified in place.
        NegateChecked = 30,
        //
        // Summary:
        //     An operation that calls a constructor to create a new object, such as new
        //     SampleType().
        New = 31,
        //
        // Summary:
        //     An operation that creates a new one-dimensional array and initializes it
        //     from a list of elements, such as new SampleType[]{a, b, c} in C# or New SampleType(){a,
        //     b, c} in Visual Basic.
        NewArrayInit = 32,
        //
        // Summary:
        //     An operation that creates a new array, in which the bounds for each dimension
        //     are specified, such as new SampleType[dim1, dim2] in C# or New SampleType(dim1,
        //     dim2) in Visual Basic.
        NewArrayBounds = 33,
        //
        // Summary:
        //     A bitwise complement or logical negation operation. In C#, it is equivalent
        //     to (~a) for integral types and to (!a) for Boolean values. In Visual Basic,
        //     it is equivalent to (Not a). The object a should not be modified in place.
        Not = 34,
        //
        // Summary:
        //     An inequality comparison, such as (a != b) in C# or (a <> b) in Visual Basic.
        NotEqual = 35,
        //
        // Summary:
        //     A bitwise or logical OR operation, such as (a | b) in C# or (a Or b) in Visual
        //     Basic.
        Or = 36,
        //
        // Summary:
        //     A short-circuiting conditional OR operation, such as (a || b) in C# or (a
        //     OrElse b) in Visual Basic.
        OrElse = 37,
        //
        // Summary:
        //     A reference to a parameter or variable that is defined in the context of
        //     the expression. For more information, see System.Linq.Expressions.ParameterExpression.
        Parameter = 38,
        //
        // Summary:
        //     A mathematical operation that raises a number to a power, such as (a ^ b)
        //     in Visual Basic.
        Power = 39,
        //
        // Summary:
        //     An expression that has a constant value of type System.Linq.Expressions.Expression.
        //     A System.Linq.Expressions.ExpressionType.Quote node can contain references
        //     to parameters that are defined in the context of the expression it represents.
        Quote = 40,
        //
        // Summary:
        //     A bitwise right-shift operation, such as (a >> b).
        RightShift = 41,
        //
        // Summary:
        //     A subtraction operation, such as (a - b), without overflow checking, for
        //     numeric operands.
        Subtract = 42,
        //
        // Summary:
        //     An arithmetic subtraction operation, such as (a - b), that has overflow checking,
        //     for numeric operands.
        SubtractChecked = 43,
        //
        // Summary:
        //     An explicit reference or boxing conversion in which null is supplied if the
        //     conversion fails, such as (obj as SampleType) in C# or TryCast(obj, SampleType)
        //     in Visual Basic.
        TypeAs = 44,
        //
        // Summary:
        //     A type test, such as obj is SampleType in C# or TypeOf obj is SampleType
        //     in Visual Basic.
        TypeIs = 45,
        //
        // Summary:
        //     An assignment operation, such as (a = b).
        Assign = 46,
        //
        // Summary:
        //     A block of expressions.
        Block = 47,
        //
        // Summary:
        //     Debugging information.
        DebugInfo = 48,
        //
        // Summary:
        //     A unary decrement operation, such as (a - 1) in C# and Visual Basic. The
        //     object a should not be modified in place.
        Decrement = 49,
        //
        // Summary:
        //     A dynamic operation.
        Dynamic = 50,
        //
        // Summary:
        //     A default value.
        Default = 51,
        //
        // Summary:
        //     An extension expression.
        Extension = 52,
        //
        // Summary:
        //     A "go to" expression, such as goto Label in C# or GoTo Label in Visual Basic.
        Goto = 53,
        //
        // Summary:
        //     A unary increment operation, such as (a + 1) in C# and Visual Basic. The
        //     object a should not be modified in place.
        Increment = 54,
        //
        // Summary:
        //     An index operation or an operation that accesses a property that takes arguments.
        Index = 55,
        //
        // Summary:
        //     A label.
        Label = 56,
        //
        // Summary:
        //     A list of run-time variables. For more information, see System.Linq.Expressions.RuntimeVariablesExpression.
        RuntimeVariables = 57,
        //
        // Summary:
        //     A loop, such as for or while.
        Loop = 58,
        //
        // Summary:
        //     A switch operation, such as switch in C# or Select Case in Visual Basic.
        Switch = 59,
        //
        // Summary:
        //     An operation that throws an exception, such as throw new Exception().
        Throw = 60,
        //
        // Summary:
        //     A try-catch expression.
        Try = 61,
        //
        // Summary:
        //     An unbox value type operation, such as unbox and unbox.any instructions in
        //     MSIL.
        Unbox = 62,
        //
        // Summary:
        //     An addition compound assignment operation, such as (a += b), without overflow
        //     checking, for numeric operands.
        AddAssign = 63,
        //
        // Summary:
        //     A bitwise or logical AND compound assignment operation, such as (a &= b)
        //     in C#.
        AndAssign = 64,
        //
        // Summary:
        //     An division compound assignment operation, such as (a /= b), for numeric
        //     operands.
        DivideAssign = 65,
        //
        // Summary:
        //     A bitwise or logical XOR compound assignment operation, such as (a ^= b)
        //     in C#.
        ExclusiveOrAssign = 66,
        //
        // Summary:
        //     A bitwise left-shift compound assignment, such as (a <<= b).
        LeftShiftAssign = 67,
        //
        // Summary:
        //     An arithmetic remainder compound assignment operation, such as (a %= b) in
        //     C#.
        ModuloAssign = 68,
        //
        // Summary:
        //     A multiplication compound assignment operation, such as (a *= b), without
        //     overflow checking, for numeric operands.
        MultiplyAssign = 69,
        //
        // Summary:
        //     A bitwise or logical OR compound assignment, such as (a |= b) in C#.
        OrAssign = 70,
        //
        // Summary:
        //     A compound assignment operation that raises a number to a power, such as
        //     (a ^= b) in Visual Basic.
        PowerAssign = 71,
        //
        // Summary:
        //     A bitwise right-shift compound assignment operation, such as (a >>= b).
        RightShiftAssign = 72,
        //
        // Summary:
        //     A subtraction compound assignment operation, such as (a -= b), without overflow
        //     checking, for numeric operands.
        SubtractAssign = 73,
        //
        // Summary:
        //     An addition compound assignment operation, such as (a += b), with overflow
        //     checking, for numeric operands.
        AddAssignChecked = 74,
        //
        // Summary:
        //     A multiplication compound assignment operation, such as (a *= b), that has
        //     overflow checking, for numeric operands.
        MultiplyAssignChecked = 75,
        //
        // Summary:
        //     A subtraction compound assignment operation, such as (a -= b), that has overflow
        //     checking, for numeric operands.
        SubtractAssignChecked = 76,
        //
        // Summary:
        //     A unary prefix increment, such as (++a). The object a should be modified
        //     in place.
        PreIncrementAssign = 77,
        //
        // Summary:
        //     A unary prefix decrement, such as (--a). The object a should be modified
        //     in place.
        PreDecrementAssign = 78,
        //
        // Summary:
        //     A unary postfix increment, such as (a++). The object a should be modified
        //     in place.
        PostIncrementAssign = 79,
        //
        // Summary:
        //     A unary postfix decrement, such as (a--). The object a should be modified
        //     in place.
        PostDecrementAssign = 80,
        //
        // Summary:
        //     An exact type test.
        TypeEqual = 81,
        //
        // Summary:
        //     A ones complement operation, such as (~a) in C#.
        OnesComplement = 82,
        //
        // Summary:
        //     A true condition value.
        IsTrue = 83,
        //
        // Summary:
        //     A false condition value.
        IsFalse = 84,
    }
}
