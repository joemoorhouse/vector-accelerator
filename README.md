# vector-accelerator
.NET library for accelerated vector maths and adjoint algorithmic differentiation (vector AAD).

# About vector-accelerator
vector-accelerator is a maths library that provides high-performance vector operations. These can be run:
* In immediate-execution mode, where vector operations are accelerated using Intel VML (hardware-tuned, vectorised operations)
* In deferred-execution mode, where the number of vector operations is minimised and local (intermediate) storage is reused to further speed up the calculation. The idea is similar to the 'numexpr' Python library, for example.

In deferred-execution mode, it is also possible to evaluate the derivative of a function with respect to specified independent variables using the efficient adjoint algorithmic differentiation (AAD) technique.

An example of how these techniques can be used to build a high-performance financial risk engine is provided.
