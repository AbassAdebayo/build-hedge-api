using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Tenant
{
    public interface ITenantProvider
    {
        Guid GetTenantId();
        string GetTenantUserName();
    }
}
