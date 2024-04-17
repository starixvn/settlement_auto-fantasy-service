using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectFantasy.Exceptions;

public class RelatedMatchesInsufficientException : AppException
{
    public RelatedMatchesInsufficientException()
    {
        Description = "Failed: Parent matches is insufficient";
    }
}