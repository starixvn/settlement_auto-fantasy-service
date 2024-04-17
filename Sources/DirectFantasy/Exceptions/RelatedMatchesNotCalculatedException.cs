using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectFantasy.Exceptions;

public class RelatedMatchesNotCalculatedException : AppException
{
    public RelatedMatchesNotCalculatedException()
    {
        Description = "Failed: Parent match is not completed";
    }
}