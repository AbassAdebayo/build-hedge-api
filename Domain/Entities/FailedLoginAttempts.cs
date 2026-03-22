using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class FailedLoginAttempts
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string IpAddress { get; set; }
        public int AttemptCount { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public DateTime RemainingSeconds { get; set; }
    }
}
