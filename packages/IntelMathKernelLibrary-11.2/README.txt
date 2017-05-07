Steps to build dll

Install MKL 2017 Community

Run as admin VS2015 x64 Native Tools Command Prompt
cd "C:\Program Files (x86)\IntelSWTools\compilers_and_libraries\windows\mkl\tools\builder"
nmake libintel64 export=vector_accelerator_list.txt threading=sequential name=mkl_vector_accelerator_x64
dumpbin mkl_vector_accelerator.dll /EXPORTS

nmake libia32 export=vector_accelerator_list.txt threading=sequential name=mkl_vector_accelerator_x86