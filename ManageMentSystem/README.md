# Management System Project

This is a Management System project built using **ASP.NET Core MVC**. It provides a comprehensive solution for managing various operations.

## Login Credentials

You can use the following default credentials to log in to the system:
- **Username / Email:** `salamhack`
- **Password:** `salamhack`

---

## Project Structure

Here is the exact folder structure of the system, based on the actual directories present in the project:

```text
ManageMentSystem/
├── Controllers/         # Handles incoming HTTP requests and returns Views
├── Data/                # Contains the database context (DbContext) and core data configurations
├── Docs/                # Project documentation files
├── Filters/             # Action filters that execute code before or after Controller actions
├── Helpers/             # Utility classes and helper functions used across the project
├── Migrations/          # Entity Framework (EF Core) database schema updates and history
├── Models/              # Represents database tables and schema entities
├── Properties/          # Environment configurations
│   └── PublishProfiles/ # Deployment and publishing settings
├── Services/            # Core business logic implementation
│   ├── AiServices/      # AI integration, Chat context, Prompting, and Tools
│   ├── AuthServices/    # Authentication and Authorization
│   ├── CategoryServices/
│   ├── CustomerAccountServices/
│   ├── CustomerServices/
│   ├── ExcelExportServices/ # Handling exports to Excel files
│   ├── GeneralDebtServices/
│   ├── HomeServices/
│   ├── InstallmentServices/ # Managing installments and payment plans
│   ├── PaymentOptionServices/
│   ├── ProductServices/
│   ├── SalesServices/
│   ├── StatisticsServices/  # Dashboard and reporting logic
│   ├── StoreAccountServices/
│   ├── SystemSettings/
│   ├── UserInvoice/
│   ├── UserServices/
│   └── WhatsAppServices/    # WhatsApp integration and messaging
├── ViewComponents/      # Independent, reusable UI components
├── ViewModels/          # Data transfer objects for passing data between Controllers and Views
├── Views/               # Contains the Razor Pages (.cshtml)
│   ├── Ai/              # Views for the AI Assistant interface
│   ├── Auth/            # Login and authentication views
│   ├── Categories/
│   ├── Customers/
│   ├── GeneralDebts/
│   ├── Home/            # Dashboard and landing views
│   ├── InstallmentPayments/
│   ├── Installments/
│   ├── PaymentMethods/
│   ├── Products/
│   ├── Reports/
│   ├── Sales/
│   ├── Shared/          # Layouts, partial views, and reusable component views
│   │   └── Components/  # Views for the ViewComponents (Categories, LowStockAlert, etc.)
│   ├── StoreAccount/
│   ├── SystemSettings/
│   ├── UserInvoice/
│   └── WhatsApp/
├── wwwroot/             # Static web assets
│   ├── css/             # Custom stylesheets
│   ├── fonts/           # Custom fonts (e.g., Cairo)
│   ├── images/          # Application images and icons
│   ├── js/              # Custom JavaScript
│   │   ├── installmentsjs/
│   │   ├── productjs/
│   │   └── salejs/
│   ├── lib/             # Third-party libraries (Bootstrap, ChartJS, DataTables, TailwindCSS, SweetAlert2, etc.)
│   └── temp/            # Temporary generated files
│
├── Program.cs           # Application entry point, dependency injection setup, and middleware pipeline
├── appsettings.json     # General application settings, including database connection strings
└── tailwind.config.js   # Configuration file for Tailwind CSS styling
```

## Key Architectural Highlights

- **MVC Pattern**: Follows the Model-View-Controller architecture to separate concerns.
- **Service-Oriented**: Business logic is strictly organized into domain-specific modules under the `Services/` directory (e.g., `SalesServices`, `AiServices`).
- **Rich UI**: The frontend utilizes libraries like DataTables, SweetAlert2, Chart.js, and is styled with **Tailwind CSS**.
