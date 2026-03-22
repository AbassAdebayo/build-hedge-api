using Application.DTOs.Auth.Validator;
using Application.Interfaces.ExchangeRate;
using Domain.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Context;
using Infrastructure.ExchangeRate;
using Infrastructure.Extensions;
using Infrastructure.HedgeBackgroundWorker;
using Infrastructure.IDS;
using Infrastructure.IOC.Extensions;
using Infrastructure.Jobs;
using Infrastructure.Services.Billing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Quartz;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration)
        .AddCors();

builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
   o.TokenLifespan = TimeSpan.FromHours(3));

builder.Services.AddDatabase(builder.Configuration.GetConnectionString("HedgeConnection")!)
    .AddMemoryCache();

// For Subscription Middleware, we need to create a scope for DbContext
builder.Services.AddDbContextFactory<BuildHedgeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HedgeConnection")), ServiceLifetime.Scoped);

builder.Services.AddHttpClient<ICurrencyExchangeService, CurrencyExchangeService>();

// Add Email Configurations
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddHostedService<HedgeLifecycleWorker>();

builder.Services.AddScoped<BillingService>();

// Billing Job Setup
// Runs 1AM on the 1st of every month
builder.Services.AddQuartz(q =>
{
    var billingKey = new JobKey("monthlyBilling");

    q.AddJob<MonthlyBillingJob>(opts => opts.WithIdentity(billingKey));

    q.AddTrigger(opts => opts
        .ForJob(billingKey)
        .WithIdentity("monthlyBillingTrigger")
        .WithCronSchedule("0 0 1 1 * ?"));
        //.WithCronSchedule("0 52 21 * * ?"));

// Payments Reminder Job Setup
// Runs every day at 9AM
var reminderKey = new JobKey("paymentReminder");

    q.AddJob<PaymentReminderJob>(opts => opts.WithIdentity(reminderKey));

    q.AddTrigger(opts => opts
        .ForJob(reminderKey)
        .WithCronSchedule("0 0 9 * * ?"));


    // Trial Cleanup Job Setup
    // Runs every 6 hours
    var cleanupKey = new JobKey("trialCleanup");

    q.AddJob<TrialCleanupJob>(opts => opts.WithIdentity(cleanupKey));

    q.AddTrigger(opts => opts
        .ForJob(cleanupKey)
        .WithCronSchedule("0 0 */6 * * ?"));
});


builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});


builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Disable default DataAnnotations validation to avoid duplicate messages
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true);


// RateLimiting Setup

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ResendEmailPolicy", httpContext =>
    {
        var email = httpContext.Request.Form["Email"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: email ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1,
                    Window = TimeSpan.FromMinutes(1)
                });
    });
        
 
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please wait a minute before trying again.", token);
    };


});

// Automatically register all validators in the project
builder.Services.AddValidatorsFromAssemblyContaining<RegisterOrganizationRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //ValidIssuer = builder.Configuration["JwtTokenSettings:TokenIssuer"],
        //ValidAudience = builder.Configuration["JwtTokenSettings:TokenIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtTokenSettings:TokenKey"])),
        RoleClaimType = ClaimTypes.Role
    };
    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
#pragma warning disable ASP0019 
                context.Response.Headers.Add("Token-Expired", "true");
#pragma warning restore ASP0019
            }
            return Task.CompletedTask;
        },

    };
});

builder.Services.AddAuthorization();
builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromHours(3));

builder.Services.AddCors(options =>
{
    options.AddPolicy("BuildHedgeClient", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BuildHedge API",
        Version = "v1",
        Description = "BuildHedge API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer' [space] and then your valid token.",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });

    c.AddSecurityDefinition("PaystackSignature", new OpenApiSecurityScheme
    {
        Description = "Enter your HMAC SHA512 hash here",
        Name = "x-paystack-signature",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("PaystackSignature", document),
            new List<string>()
        }
    });

    c.CustomSchemaIds(x => x.FullName);

});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BuildHedge API V1");
    //c.RoutePrefix = string.Empty;
});

app.UseRouting();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseMiddleware<SubscriptionMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
