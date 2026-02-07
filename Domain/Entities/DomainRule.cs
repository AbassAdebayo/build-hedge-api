using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class DomainRule : BaseEntity
    {
        [Required]
        public string DomainName { get; set; } = string.Empty;

        public bool IsAllowed { get; set; }

        public string? Note { get; set; }
    }
}
