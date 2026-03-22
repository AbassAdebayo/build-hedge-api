using Application.DTOs.IDS;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.IDS
{
    public interface IIdsService
    {
        public Task RegisterFailedAttemptAsync(string ipAddress);
        public Task ResetLoginAttemptsAsync(string ipAddress);
        public Task<LoginAttemptResult> CheckLoginAttemptAsync(string ipAddress);


    }
}
