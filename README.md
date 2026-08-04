# Build Hedge API

**Construction Material Price Hedging Platform**

Build Hedge is a multi-tenant SaaS API that helps construction companies **lock material prices** (hedge contracts), manage projects and budgets, track material price history, handle multi-currency exchange rates, and automate billing/subscriptions.

---

## Overview

Build Hedge enables organizations to:

- Register organizations and manage multi-user memberships
- Create construction projects with budgets
- Define materials (with ticker symbols and units)
- Lock material prices via **Hedge Contracts** (quantity, locked price, premium, expiry)
- Track material price history
- Handle multi-currency with exchange rates at lock time
- Generate billing statements (subscription + premium + overage fees)
- Run background jobs for billing, payment reminders, and trial cleanup
- Generate PDF invoices
- Invite users and manage roles

### Key Features

- Multi-tenant organizations with subscription plans & trials
- Projects linked to organizations
- Materials catalog with price history
- Hedge contracts (lock price, quantity, premium, expiry notices)
- Multi-currency support + exchange rate service
- Automated monthly billing & payment reminders (Quartz jobs)
- PDF generation (QuestPDF)
- JWT authentication
- Role-based access
- Email notifications (invite, verification, invoice, forgot password)
- Rate limiting
- FluentValidation

---

## Tech Stack

| Layer              | Technology                                      |
|--------------------|-------------------------------------------------|
| Framework          | ASP.NET Core (.NET 10)                          |
| Architecture       | Clean Architecture (Domain → Application → Infrastructure → Api) |
| Database           | SQL Server + EF Core                            |
| Auth               | JWT Bearer + ASP.NET Identity                   |
| Background Jobs    | Quartz                                          |
| PDF                | QuestPDF                                        |
| Email              | Brevo / MailKit / Mailchimp                     |
| Validation         | FluentValidation                                |
| API Docs           | Swagger / OpenAPI                               |

---

## Solution Structure

```
build-hedge-api/
├── Api/                         # Web API host
│   ├── Controllers/             # Auth, Organization, User, Role, Billing, Currency, OwnerSettings
│   └── Views/                   # Email templates
├── Application/                 # DTOs, Interfaces, Validators
├── Domain/                      # Entities, Enums, Contracts
│   └── Entities/                # Organization, Project, Material, HedgeContract, etc.
└── Infrastructure/              # EF Core, Identity, Jobs, Services, Tenant, PDF, Messaging
    ├── Context/
    ├── Identity/
    ├── Jobs/                    # MonthlyBilling, PaymentReminder, TrialCleanup
    ├── ExchangeRate/
    ├── PdfHandler/
    ├── Tenant/
    └── HedgeBackgroundWorker/
```

---

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server
- Visual Studio 2022 / Rider / VS Code

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd build-hedge-api
```

### 2. Configure settings

Update `Api/appsettings.json` or use User Secrets:

```json
{
  "ConnectionStrings": {
    "HedgeConnection": "Server=...;Database=BuildHedge;..."
  },
  "JwtTokenSettings": {
    "TokenKey": "YOUR_SECRET_KEY",
    "TokenExpiryPeriod": "..."
  }
}
```

### 3. Apply migrations

```bash
cd Api
dotnet ef database update --project ../Infrastructure
```

### 4. Run the API

```bash
dotnet run --project Api
```

Swagger UI will be available at the configured URL.

---

## Core Domain Entities

| Entity                  | Description                                              |
|-------------------------|----------------------------------------------------------|
| `Organization`          | Tenant/company with subscription plan, trial, currency   |
| `Project`               | Construction project with budget & completion date       |
| `Material`              | Material with ticker symbol, unit, price history         |
| `HedgeContract`         | Locked price contract (qty, premium, expiry, status)     |
| `MaterialPriceHistory`  | Historical prices for materials                          |
| `Currency`              | Supported currencies                                     |
| `BillingStatement`      | Invoice (subscription + premiums + overages)             |
| `ProcessedPayment`      | Payment records                                          |
| `User` / `Role`         | Users and roles within organizations                     |
| `UserOrganizationMembership` | User ↔ Organization membership                      |
| `GlobalSettings` / `DomainRule` | Platform-level settings                           |

---

## Main Modules

### Auth & Organizations
- Register organization + admin
- Invite users to organization
- JWT login
- Role management

### Projects & Materials
- Create projects under an organization
- Manage materials and price history

### Hedging
- Create hedge contracts (lock material price)
- Track expiry notices (7-day, final)
- Calculate premium and overage fees

### Billing
- Monthly billing job
- Billing statements / invoices (PDF)
- Payment reminders
- Trial cleanup

### Currency
- Multi-currency support
- Exchange rate at contract lock time

---

## Documentation

- [Architecture & System Diagrams](docs/architecture.md) – High-level architecture, hedge flow, billing, entity relationships, and module overview.

---

## Background Jobs (Quartz)

| Job                   | Purpose                              |
|-----------------------|--------------------------------------|
| `MonthlyBillingJob`   | Generate monthly invoices            |
| `PaymentReminderJob`  | Remind unpaid statements             |
| `TrialCleanupJob`     | Handle expired trials                |

---

## Roadmap

- [x] Multi-tenant organizations & memberships
- [x] Projects & materials
- [x] Hedge contracts
- [x] Billing & PDF invoices
- [x] Currency & exchange rates
- [x] Background jobs
- [ ] Real-time market price feeds
- [ ] Advanced analytics dashboard
- [ ] Mobile notifications
- [ ] Payment gateway integration

---

## Contributing

1. Create a feature branch
2. Follow Clean Architecture boundaries
3. Add validators for new requests
4. Submit a Pull Request

---

## License

Proprietary – All rights reserved.

---

**Built with** ASP.NET Core, EF Core, Quartz, QuestPDF, and SQL Server.
