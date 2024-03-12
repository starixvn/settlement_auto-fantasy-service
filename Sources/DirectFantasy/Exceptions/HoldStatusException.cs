using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DirectFantasy.Exceptions;

[Serializable]
public class HoldStatusException : AppException
{
    public HoldStatusException()
    {
        Description = "Failed: Cannot execute auto-settle process due to match is on hold.";
    }
}