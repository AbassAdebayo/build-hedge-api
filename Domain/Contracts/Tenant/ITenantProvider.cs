using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Tenant
{
    public interface ITenantProvider
    {
        Guid GetTenantId();
        string GetTenantUserName();
        public Guid GetTenantUserId();
    }
}
