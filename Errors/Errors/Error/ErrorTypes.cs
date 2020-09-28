using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Error
{
    public enum ErrorTypes
    {
        ProgramError,
        OutOfRange,
        NegativeTime,
        DynamicCompileError,
        StrangeStephanError,
        FileNameEmpty,
        FileNotFound,
        ExternalError,
        Other
    }
}
