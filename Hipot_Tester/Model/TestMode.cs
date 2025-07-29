using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipot_Tester.Model
{
    public enum TestMode
    {
        AC,
        DC,
        IR
    }

    public enum TestRange
    {
        Auto,
        Range10mA,
        Range3mA,
        Range300uA,
        Range30uA,
        Range3uA,
        Range300nA
    }
}
