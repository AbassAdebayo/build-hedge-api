using Domain.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Currency : BaseEntity
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
