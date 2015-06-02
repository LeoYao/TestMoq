using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMoq
{
    public interface IMockTarget
    {
        void VoidReturnMethod();
        int  NonvoidReturnMethod();
        int  ParameterizedMethod(int v1);
    }
}
