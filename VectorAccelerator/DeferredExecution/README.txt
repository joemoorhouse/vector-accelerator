1) NArray storage
NArray storage can be either on host, which is then always managed storage, or on device.
A typical use case is first building code to run on host and then running on device. We do not
therefore want to make the user have to specify the storage target when creating NArrays.
Implicit creation can be handled, for explicit creation a couple of options spring to mind:
a) Use an NArrayFactory to create NArrays (the factory knows where the storage should be)
b) Encourage use of CreateLike - using an existing NArray as a template

We try to push b) to start with, but can have both (just do not expose NArrayFactory).

2) Immediate and Deferred Execution
Execution of operations can be immediate, but a deferred execution mode is available for 
deferrable (typically vector-vector of the same size) operations. 

Here the vector operations that are to be run are stored until either the user requests
that these are run or a non-deferrable operation is requested. The deferrred operations are
then run all together, taking advantage of any optimisations thereby made possible.

Optimisations are typically:
a) On CPU, re-use of temporary storage, reducing memory allocation and improving cache use
b) On GPU, the same, but in addition the ability to compile an optimised kernel

2.1) Storage of Deferred Operations
Deferred operations are storaged as a list of NArrayOperations.

The NArrayOperation is broadly similar to a System.Linq.Expression. 
However, a BinaryExpression will have only Left and Right operands, a group of Expressions 
forming a tree. A BinaryVectorOperation also has a Result: it is equivalent to a BinaryExpression 
and an Assignment in System.Linq.Expression terms.
e.g. temp1 = a * b

The starting point is a flat list of NArrayOperations. Perhaps additional functionality will be
required which will require a form of Expression Tree (e.g. loops of vector operations)
but we will start simple and then extend if necessary.

2.2) Executors
An Executor is an object that can execute an operation.
  
The main Executor is the Executor class itself. The Executor decides whether to execute an operation
immediately (calling on the ImmediateExecutor to do so) or whether to defer execution, making use
of the DeferredOperationsStore. It will then run the DeferredOperations using an appropriate 
DeferredOperationsRunner.

The actual arithmetic - vector operations, matrix multiplications, matrix decompositions etc - 
is outsourced to ILinearAlgebraProviders (ones for CPU and for GPGPU).

2.3) Specifying deferred operations
Is it possible to get the system to decide what can be deferred and what cannot be? There is a problem,
which is that we want temporary vectors created for deferred operations to be, well, temporary.

e.g.
var temp = (a + b) * NMath.Exp(a); // temp is a temporary 
result.Assign(d * temp); // this is a matrix multiply that uses the temp vector, so temp must be real

To start with, deferring will be strict (no non-deferrable operations allowed); we can later relax this.

3) Compilations of kernels
