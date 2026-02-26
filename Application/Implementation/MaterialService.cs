using Application.DTOs;
using Application.DTOs.Material;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Contracts.Tenant;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Implementation
{
    public class MaterialService(IMaterialRepository materialRepository,  
       IUnitOfWork unitOfWork, ILogger<MaterialService> logger, ITenantProvider tenantProvider) : IMaterialService
    {
        private readonly Guid _tenantProvider = tenantProvider.GetTenantId();
        private readonly string _tenantUserName = tenantProvider.GetTenantUserName();
        private readonly ILogger<MaterialService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMaterialRepository _materialRepository = materialRepository ?? throw new ArgumentNullException(nameof(materialRepository));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        public async Task<BaseResponse<bool>> CreateMaterialAsync(CreateMaterialRequestModel request)
        {
            var existingMaterial = await _materialRepository.Any<Material>(m => m.TickerSymbol == request.TickerSymbol);
            if (existingMaterial)
                return new BaseResponse<bool>("Material already exists in your registry", false, false);

            var material = new Material
            {
                Name = request.Name,
                TickerSymbol = request.TickerSymbol,
                MetadataJson = request.MetadataJson,
                Unit = request.Unit,
                OrganizationId = _tenantProvider,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = _tenantUserName
            };

            await _materialRepository.Add<Material>(material);
            return await _unitOfWork.SaveChangesAsync() > 0 ? new BaseResponse<bool>(
                "Material registered", true, true) : new BaseResponse<bool>(
                    "Material couldn't be registered", false, false);
        }

        public async Task<BaseResponse<IEnumerable<ListOfMaterialsResponse>>> GetAllMaterialsAsync()
        {
            var materials = await _materialRepository.GetAll<Material>();
            if(materials is null || !materials.Any())
                return new BaseResponse<IEnumerable<ListOfMaterialsResponse>>(
                    "No materials found", false, null!);

            var materailsData = materials.Select(m => new ListOfMaterialsResponse(
                m.Id,
                m.Name,
                m.TickerSymbol,
                m.Unit,
                m.MetadataJson,
                m.RowVersion,
                m.CreatedBy ?? ""

             )).ToList();

            return new BaseResponse<IEnumerable<ListOfMaterialsResponse>>(
                $"{materailsData.Count} Materials retrieved successfully", true, materailsData);

        }
    }
}
