using Application.Interfaces.Repositories;
using Domain.Contracts.Enum;
using Domain.Contracts.MailingServices;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;  

namespace Infrastructure.HedgeBackgroundWorker
{
    public class HedgeLifecycleWorker(IServiceProvider serviceProvider,
        ILogger<HedgeLifecycleWorker> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly ILogger<HedgeLifecycleWorker> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    await ManageHedgeLifecyclesAsync();
            //    await Task.Delay(_checkInterval, stoppingToken);
            //}

            await Task.Yield();

            _logger.LogInformation("BuildHedge Lifecycle Worker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    try
                    {
                        var hedgeContext = scope.ServiceProvider.GetRequiredService<IHedgeContractRepository>();
                        var notificationContext = scope.ServiceProvider.GetRequiredService<IMailService>();
                        var userContext = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var materialContext = scope.ServiceProvider.GetRequiredService<IMaterialRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        _logger.LogInformation("Running lifecycle check...");
                        await ManageHedgeLifecyclesAsync(hedgeContext, notificationContext, userContext, materialContext, unitOfWork);
                        _logger.LogInformation("Lifecycle check completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred in HedgeLifecycleWorker");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }
        }

        private async Task ManageHedgeLifecyclesAsync(IHedgeContractRepository hedgeContext, 
            IMailService notificationContext,
            IUserRepository userContext,
            IMaterialRepository materialContext,
            IUnitOfWork unitOfWork
            )
       {
            
                
                var today = DateTime.UtcNow.Date;
                
                // (Already Expired)
                var expired = await hedgeContext.GetAll<HedgeContract>(
                    hc => hc.Status == Domain.Contracts.Enum.ContractStatus.Active &&
                    hc.ExpiryDate < today &&
                    !hc.IsExpiredNoticeSentAt.HasValue,
                    ignoreFilters: true
                 );


                foreach (var hedge in expired)
                {
                    hedge.Status = Domain.Contracts.Enum.ContractStatus.Expired;

                    var user = await userContext.Get<User>(u => u.Id == hedge.CreatedByUserId);
                    var material = await materialContext.Get<Material>(m => m.Id == hedge.MaterialId, ignoreFilters: true);

                    _logger.LogInformation($"Processing hedge {hedge.Id} for user {hedge.CreatedByUserId}");

                    if (user is not null && material is not null)
                    {
                        var sent = await notificationContext.SendNotificationMail(
                            user.Email, 
                            user.FirstName,
                            "Price Lock Expired",
                            $"Your hedge for material {material.Name} has expired. The locked price is no longer valid."
                       );
                    if (sent)
                    {
                        hedge.IsExpiredNoticeSentAt = DateTime.UtcNow;
                        await hedgeContext.Update<HedgeContract>(hedge);
                    }
                        
                }
                    else
                    {
                        _logger.LogWarning($"Could not send notification. User: {user?.Id}, Material: {material?.Id}");
                    }

                }

                // The Warning(Expiring in 7 days)
                var warningDate = today.AddDays(7);
                var startOfTheFinalDay = warningDate;
                var endOfTheFinalDay = warningDate.AddDays(1).AddTicks(-1);

                var sevenDayWarnings = await hedgeContext.GetAll<HedgeContract>(h =>
                    h.Status == ContractStatus.Active &&
                    h.ExpiryDate.Date >= startOfTheFinalDay &&
                    h.ExpiryDate <= endOfTheFinalDay &&
                   !h.IsSevenDayNoticeSent.HasValue,
                   ignoreFilters: true);

                foreach (var hedge in sevenDayWarnings)
                {
                    var user = await userContext.Get<User>(u => u.Id == hedge.CreatedByUserId);
                    var material = await materialContext.Get<Material>(m => m.Id == hedge.MaterialId, ignoreFilters: true);

                    _logger.LogInformation($"Processing hedge {hedge.Id} for user {hedge.CreatedByUserId}");

                    if (user is not null && material is not null)
                    {
                        var sent = await notificationContext.SendNotificationMail(
                         user.Email,
                         user.FirstName,
                         "Action Required: 7 Days Left",
                         $"Your price lock for {material.Name} will expire in 7 days. Ensure your procurement is finalized soon."
                        );

                        if (sent)
                        {
                            hedge.IsSevenDayNoticeSent = DateTime.UtcNow;
                         await hedgeContext.Update<HedgeContract>(hedge);
                        }

                    }
                    else
                    {
                        _logger.LogWarning($"Could not send notification. User: {user?.Id}, Material: {material?.Id}");
                    }

                }

                // The Final Alert (Expiring tomorrow)
                var finalDate = today.AddDays(1);
                var lastCall = await hedgeContext.GetAll<HedgeContract>(h =>
                    h.Status == ContractStatus.Active &&
                    h.ExpiryDate.Date == finalDate &&
                    !h.IsFinalNoticeSent.HasValue,
                    ignoreFilters: true
                 );

                foreach (var hedge in lastCall)
                {
                    var user = await userContext.Get<User>(u => u.Id == hedge.CreatedByUserId);
                    var material = await materialContext.Get<Material>(m => m.Id == hedge.MaterialId, ignoreFilters: true);

                    _logger.LogInformation($"Processing hedge {hedge.Id} for user {hedge.CreatedByUserId}");

                    if (user is not null && material is not null)
                    {
                        var sent = await notificationContext.SendNotificationMail(user.Email, 
                            user.FirstName,
                            "URGENT: 24 Hours Remaining",
                            $"This is your final notice. Your price lock for {material.Name} expires tomorrow. Use it now to maintain your project margins."
                         );
                        if (sent)
                        {
                            hedge.IsFinalNoticeSent = DateTime.UtcNow;
                        await hedgeContext.Update<HedgeContract>(hedge);
                    }
                    }
                    else
                    {
                        _logger.LogWarning($"Could not send notification. User: {user?.Id}, Material: {material?.Id}");
                    }
                }
                
            await unitOfWork.SaveChangesAsync();
            

        }

    }
}
