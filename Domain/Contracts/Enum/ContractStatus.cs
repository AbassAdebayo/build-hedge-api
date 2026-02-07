using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Enum
{
    public enum ContractStatus
    {
        Active = 1,
        Expired,
        Claimed,
        Cancelled
    }
}
