using Application.Tenant;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Tenant
{
    public class TenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        public string GetTenantUserName()
        {
            
            var claimUserName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            return claimUserName ?? string.Empty;
        }

        public Guid GetTenantId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("CurrentOrgId")?.Value;
            return Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}
