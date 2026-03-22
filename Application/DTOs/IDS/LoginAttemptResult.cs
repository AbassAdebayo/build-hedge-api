using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.IDS
{
    public record LoginAttemptResult(
        bool IsBlocked,
        DateTime? BlockedUntil,
        int? RemainingSeconds
        );
}
