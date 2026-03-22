using Application.DTOs.IDS;
using Application.Interfaces.IDS;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.IDS
{
    public class IdsService(BuildHedgeContext context) : IIdsService
    {
        private readonly BuildHedgeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        public async Task RegisterFailedAttemptAsync(string ipAddress)
        {
            var record = await _context.Set<FailedLoginAttempts>()
                .FirstOrDefaultAsync(l => l.IpAddress == ipAddress);

            if (record == null)
            {
                record = new FailedLoginAttempts
                {
                    IpAddress = ipAddress,
                    AttemptCount = 1,
                    LastAttemptTime = DateTime.UtcNow
                };

                _context.Add(record);
            }
            else
            {
                record.AttemptCount++;
                record.LastAttemptTime = DateTime.UtcNow;

                if (record.AttemptCount >= 10)
                {
                    record.BlockedUntil = DateTime.UtcNow.AddHours(1);
                }
                else if (record.AttemptCount >= 5)
                {
                    record.BlockedUntil = DateTime.UtcNow.AddMinutes(15);
                }

            }
            await _context.SaveChangesAsync();

        } 
        public async Task ResetLoginAttemptsAsync(string ipAddress)
        {
            var record = await _context.Set<FailedLoginAttempts>()
                .FirstOrDefaultAsync(x => x.IpAddress == ipAddress);

            if (record != null)
            {
                _context.Remove(record);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LoginAttemptResult> CheckLoginAttemptAsync(string ipAddress)
        {
            var record = await _context.Set<FailedLoginAttempts>()
                .FirstOrDefaultAsync(x => x.IpAddress == ipAddress);

            if (record == null)
                return new LoginAttemptResult(IsBlocked: false, null, null );

            if (record.BlockedUntil != null && record.BlockedUntil > DateTime.UtcNow)
            {
                return new LoginAttemptResult
                (
                    IsBlocked: true,
                    BlockedUntil: record.BlockedUntil.Value,
                    RemainingSeconds: Math.Max(0, (int)(record.BlockedUntil.Value - DateTime.UtcNow).TotalSeconds)
                );
            }

            return new LoginAttemptResult(IsBlocked: false, null, null);
        }
    }
    
}
