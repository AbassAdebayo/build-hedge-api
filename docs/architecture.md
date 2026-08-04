# Build Hedge API – Architecture & System Diagrams

This document contains the system architecture and key flow diagrams for the Build Hedge material price hedging platform.

---

## 1. High-Level Architecture

```mermaid
flowchart TB
    subgraph Clients["Clients"]
        Web["Web Dashboard"]
        Admin["Platform Owner"]
    end

    subgraph API["Build Hedge Api"]
        Controllers["Controllers<br/>Auth · Org · User · Role<br/>Billing · Currency · Settings"]
        Middleware["Middleware · Rate Limiting"]
        Auth["JWT Authentication"]
    end

    subgraph Application["Application"]
        DTOs["DTOs + Validators"]
        Interfaces["Service Interfaces"]
    end

    subgraph Domain["Domain"]
        Orgs["Organization · Project"]
        Hedge["Material · HedgeContract"]
        Billing["BillingStatement · Payment"]
        Users["User · Role · Membership"]
    end

    subgraph Infrastructure["Infrastructure"]
        DbContext["EF Core Context"]
        Jobs["Quartz Jobs"]
        Exchange["Exchange Rate Service"]
        PDF["QuestPDF"]
        Mail["Email Service"]
        Tenant["Tenant Provider"]
    end

    subgraph External["External"]
        SQL[(SQL Server)]
        EmailProv["Email Provider"]
        FX["FX / Market Data"]
    end

    Clients --> Controllers
    Controllers --> Application
    Application --> Domain
    Application --> Infrastructure
    Infrastructure --> SQL
    Infrastructure --> EmailProv
    Exchange --> FX
    Jobs --> Infrastructure
```

---

## 2. Clean Architecture Layers

```mermaid
graph TD
    A[Api] --> B[Application]
    B --> C[Domain]
    D[Infrastructure] --> C
    A --> D
    B --> D

    style C fill:#f9f,stroke:#333
    style B fill:#bbf,stroke:#333
    style A fill:#bfb,stroke:#333
    style D fill:#fbb,stroke:#333
```

---

## 3. Domain Entity Relationships

```mermaid
erDiagram
    ORGANIZATION ||--o{ PROJECT : has
    ORGANIZATION ||--o{ USERORGANIZATIONMEMBERSHIP : has
    ORGANIZATION ||--o{ HEDGECONTRACT : owns
    ORGANIZATION ||--o{ BILLINGSTATEMENT : billed_by
    ORGANIZATION ||--o{ MATERIAL : catalogs

    PROJECT ||--o{ HEDGECONTRACT : contains
    MATERIAL ||--o{ HEDGECONTRACT : locked_in
    MATERIAL ||--o{ MATERIALPRICEHISTORY : has

    HEDGECONTRACT }o--|| CURRENCY : priced_in
    USER ||--o{ USERORGANIZATIONMEMBERSHIP : belongs_to
    USER ||--o{ USERROLE : has
    ROLE ||--o{ USERROLE : assigned

    ORGANIZATION {
        string BusinessName
        SubscriptionPlan SubscriptionPlan
        string BaseCurrencyCode
        bool IsInTrial
        decimal AccruedFees
    }

    HEDGECONTRACT {
        decimal LockedPrice
        decimal Quantity
        decimal PremiumFee
        decimal OverageFee
        DateTime ExpiryDate
        ContractStatus Status
        decimal ExchangeRateAtLock
    }

    MATERIAL {
        string Name
        string TickerSymbol
        string Unit
    }

    BILLINGSTATEMENT {
        string InvoiceNumber
        decimal SubscriptionBaseFee
        decimal TotalPremiumFees
        decimal TotalOverageFees
        bool IsPaid
    }
```

---

## 4. Hedge Contract Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Draft : Create contract
    Draft --> Active : Confirm / Lock price
    Active --> ExpiringSoon : 7-day notice
    ExpiringSoon --> Expired : Past expiry
    Active --> Settled : Successfully closed
    Expired --> [*]
    Settled --> [*]

    note right of Active
        LockedPrice fixed
        PremiumFee charged
        ExchangeRateAtLock stored
    end note
```

### Create Hedge Sequence

```mermaid
sequenceDiagram
    participant User
    participant API
    participant Service
    participant Exchange
    participant DB

    User->>API: Create HedgeContract
    API->>Service: Validate org/project/material
    Service->>Exchange: Get current rate
    Service->>Service: Calculate premium & total value
    Service->>DB: Save HedgeContract (Active)
    Service->>DB: Update accrued fees
    DB-->>User: Contract created
```

---

## 5. Billing Flow

```mermaid
flowchart LR
    A[Quartz MonthlyBillingJob] --> B[Load Organizations]
    B --> C[Aggregate Premium + Overage]
    C --> D[Create BillingStatement]
    D --> E[Generate PDF Invoice]
    E --> F[Send Invoice Email]
    F --> G[PaymentReminderJob]
    G --> H{Paid?}
    H -->|No| G
    H -->|Yes| I[Mark ProcessedPayment]
```

---

## 6. Multi-Tenancy / Organization Model

```mermaid
flowchart TB
    Org[Organization]
    Org --> Projects[Projects]
    Org --> Materials[Materials]
    Org --> Hedges[Hedge Contracts]
    Org --> Members[User Memberships]
    Org --> Billing[Billing Statements]
    Org --> Plan[Subscription Plan + Trial]
```

---

## 7. Module Overview

```mermaid
flowchart TB
    subgraph Core["Core"]
        Auth[JWT Auth]
        Orgs[Organizations]
        Users[Users & Roles]
    end

    subgraph Operations["Operations"]
        Projects[Projects]
        Materials[Materials + Price History]
        Hedges[Hedge Contracts]
    end

    subgraph Finance["Finance"]
        Currency[Currency + FX]
        Billing[Billing Statements]
        PDF[PDF Invoices]
        Payments[Processed Payments]
    end

    subgraph Jobs["Background Jobs"]
        Monthly[Monthly Billing]
        Reminders[Payment Reminders]
        Trial[Trial Cleanup]
    end

    Auth --> Orgs
    Orgs --> Projects
    Orgs --> Materials
    Projects --> Hedges
    Materials --> Hedges
    Hedges --> Billing
    Currency --> Hedges
    Billing --> PDF
    Jobs --> Billing
```

---

## 8. Future Enhancements

```mermaid
mindmap
  root((Build Hedge))
    Current
      Multi-tenant Orgs
      Projects & Materials
      Hedge Contracts
      Billing + PDF
      FX Rates
      Quartz Jobs
    Next
      Live market feeds
      Payment gateway
      Analytics dashboard
    Later
      Mobile app
      AI price forecasts
      Marketplace
```

---

**Notes**

- Diagrams are written in Mermaid and render on GitHub, GitLab, Notion, and VS Code.
- Update this file as new features are added.
