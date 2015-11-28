NArray principles of use

The NArray is designed to enforce use of vector operations. As such, it does not support indexing; or rather,
indexing is only allowed in certain circumstances. Any vector operation assumes contiguous storage.

Making the data within an NArray immutable is not compatible with high performance in all cases. But this is
restricted: the only way to change data is via an 'Assign' (or in-place operation). Note that using a '+=' operator gives the
usual CSharp behaviour: a '+' operation is performed, creating a new NArray and the new NArray is assigned to the left hand side.

Slicing (will be) supported and will reference the same data as the NArray it is derived from in general. However, if the slice
is an array, this is only permitted if the resulting slice memory is contiguous. Otherwise, an error is thrown. An alternative
DeepSlice is provided which performs a deep copy of the underlying data. The aim, as always, is to try and enforce code that
can be vectorised efficiently. 

1	2	3	4
5	6	7	8
9	10	11	12
14	14	15	16

take [1:2, 1:3]^T
i.e.
6	10
7	11
8	12
