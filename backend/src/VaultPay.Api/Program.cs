using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Accounts;
using VaultPay.Application.DTOs.Admin;
using VaultPay.Application.DTOs.Auth;
using VaultPay.Application.DTOs.Bills;
using VaultPay.Application.DTOs.Deposits;
using VaultPay.Application.DTOs.Loans;
using VaultPay.Application.DTOs.Savings;
using VaultPay.Application.DTOs.Transactions;
using VaultPay.Application.DTOs.Transfers;
using VaultPay.Application.Services;
using VaultPay.Domain.Entities;
using VaultPay.Infrastructure.Data;
using VaultPay.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=vaultpay.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// 2. Identity Configuration
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<Role>()
.AddRoleManager<RoleManager<Role>>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager();

// 3. JWT Authentication & Authorization
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "VaultPaySuperSecretKeyForDevelopmentAndTesting2026@#$!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "VaultPayApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "VaultPayClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomer", policy => policy.RequireRole("Customer", "Admin"));
});

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 5. HttpContext & Infrastructure Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

// 6. Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IStatementService, StatementService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<ISavingsService, SavingsService>();
builder.Services.AddScoped<ILoanService, LoanService>();

// 7. Swagger / OpenAPI with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VaultPay Core Banking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed Database Automatically
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await DbInitializer.SeedAsync(context, userManager, roleManager);
}

// HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalFrontend");
app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------------------------------
// Endpoints Definition
// --------------------------------------------------------------------------

// Health
app.MapGet("/", () => Results.Ok(new { system = "VaultPay Core Banking API", status = "Healthy", timestamp = DateTime.UtcNow }));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", db = "Connected" }));

// 1. Auth Endpoints
var authGroup = app.MapGroup("/api/auth").WithTags("Auth");

authGroup.MapPost("/register", async (RegisterRequest req, IAuthService authService) =>
{
    try
    {
        var result = await authService.RegisterAsync(req);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

authGroup.MapPost("/login", async (LoginRequest req, IAuthService authService) =>
{
    try
    {
        var result = await authService.LoginAsync(req);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

authGroup.MapGet("/me", async (IAuthService authService) =>
{
    try
    {
        var profile = await authService.GetCurrentUserProfileAsync();
        return Results.Ok(profile);
    }
    catch
    {
        return Results.Unauthorized();
    }
}).RequireAuthorization();

// 2. Dashboard Endpoints
var dashboardGroup = app.MapGroup("/api/dashboard").WithTags("Dashboard").RequireAuthorization();

dashboardGroup.MapGet("/summary", async (IDashboardService dashboardService) =>
{
    try
    {
        var summary = await dashboardService.GetSummaryAsync();
        return Results.Ok(summary);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

dashboardGroup.MapGet("/analytics", async (IDashboardService dashboardService) =>
{
    try
    {
        var analytics = await dashboardService.GetAnalyticsAsync();
        return Results.Ok(analytics);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 3. Accounts Endpoints
var accountsGroup = app.MapGroup("/api/accounts").WithTags("Accounts").RequireAuthorization();

accountsGroup.MapGet("/", async (IAccountService accountService) =>
{
    var accounts = await accountService.GetUserAccountsAsync();
    return Results.Ok(accounts);
});

accountsGroup.MapGet("/{id:guid}", async (Guid id, IAccountService accountService) =>
{
    try
    {
        var account = await accountService.GetAccountByIdAsync(id);
        return Results.Ok(account);
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

accountsGroup.MapGet("/{id:guid}/balance", async (Guid id, IAccountService accountService) =>
{
    try
    {
        var balance = await accountService.GetAccountBalanceAsync(id);
        return Results.Ok(balance);
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

accountsGroup.MapPost("/", async (CreateAccountRequest req, IAccountService accountService) =>
{
    try
    {
        var created = await accountService.CreateAccountAsync(req);
        return Results.Created($"/api/accounts/{created.Id}", created);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

accountsGroup.MapGet("/lookup/{nuban}", async (string nuban, IAccountService accountService) =>
{
    try
    {
        var recipient = await accountService.LookupRecipientAsync(nuban);
        return Results.Ok(recipient);
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 4. Transfers Endpoints
var transfersGroup = app.MapGroup("/api/transfers").WithTags("Transfers").RequireAuthorization();

transfersGroup.MapPost("/", async (TransferRequest req, HttpContext http, ITransferService transferService) =>
{
    try
    {
        var idempotencyKey = http.Request.Headers["X-Idempotency-Key"].ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            idempotencyKey = Guid.NewGuid().ToString("N");
        }

        var result = await transferService.ProcessTransferAsync(req, idempotencyKey);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 5. Deposits Endpoints
var depositsGroup = app.MapGroup("/api/deposits").WithTags("Deposits").RequireAuthorization();

depositsGroup.MapPost("/", async (DepositRequest req, HttpContext http, IDepositService depositService) =>
{
    try
    {
        var idempotencyKey = http.Request.Headers["X-Idempotency-Key"].ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            idempotencyKey = Guid.NewGuid().ToString("N");
        }

        var result = await depositService.ProcessDepositAsync(req, idempotencyKey);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 6. Transactions Endpoints
var txnsGroup = app.MapGroup("/api/transactions").WithTags("Transactions").RequireAuthorization();

txnsGroup.MapGet("/", async (
    Guid? accountId, 
    string? type, 
    string? status, 
    DateTime? fromDate, 
    DateTime? toDate, 
    int? pageNumber, 
    int? pageSize, 
    ITransactionService txnService) =>
{
    var filter = new TransactionFilterParams
    {
        AccountId = accountId,
        PageNumber = pageNumber ?? 1,
        PageSize = pageSize ?? 20
    };

    var results = await txnService.GetTransactionsAsync(filter);
    return Results.Ok(results);
});

txnsGroup.MapGet("/{id:guid}/receipt", async (Guid id, ITransactionService txnService) =>
{
    try
    {
        var receipt = await txnService.GetTransactionReceiptAsync(id);
        return Results.Ok(receipt);
    }
    catch (Exception ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

// 7. Admin Endpoints (STRICTLY REQUIRE ADMIN ROLE)
var adminGroup = app.MapGroup("/api/admin")
    .WithTags("Admin")
    .RequireAuthorization("RequireAdmin");

adminGroup.MapGet("/stats", async (IAdminService adminService) =>
{
    try
    {
        var stats = await adminService.GetStatsAsync();
        return Results.Ok(stats);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapGet("/users", async (int? pageNumber, int? pageSize, string? search, IAdminService adminService) =>
{
    try
    {
        var users = await adminService.GetUsersAsync(pageNumber ?? 1, pageSize ?? 20, search);
        return Results.Ok(users);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
});

adminGroup.MapGet("/accounts", async (int? pageNumber, int? pageSize, string? search, IAdminService adminService) =>
{
    try
    {
        var accounts = await adminService.GetAccountsAsync(pageNumber ?? 1, pageSize ?? 20, search);
        return Results.Ok(accounts);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
});

adminGroup.MapGet("/transactions", async (int? pageNumber, int? pageSize, string? search, IAdminService adminService) =>
{
    try
    {
        var txns = await adminService.GetTransactionsAsync(pageNumber ?? 1, pageSize ?? 20, search);
        return Results.Ok(txns);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
});

adminGroup.MapGet("/users/search", async (string query, IAdminService adminService) =>
{
    try
    {
        var details = await adminService.SearchUserWithTransactionsAsync(query);
        return Results.Ok(details);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapPost("/users/{id:guid}/ban", async (Guid id, IAdminService adminService) =>
{
    try
    {
        var result = await adminService.BanUserAsync(id);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapPost("/users/{id:guid}/unban", async (Guid id, IAdminService adminService) =>
{
    try
    {
        var result = await adminService.UnbanUserAsync(id);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapPut("/users/{id:guid}", async (Guid id, EditUserRequest req, IAdminService adminService) =>
{
    try
    {
        var result = await adminService.EditUserAsync(id, req);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapDelete("/users/{id:guid}", async (Guid id, IAdminService adminService) =>
{
    try
    {
        await adminService.DeleteUserAsync(id);
        return Results.Ok(new { message = "User deleted successfully." });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapPost("/users/{id:guid}/tier", async (Guid id, UpgradeTierRequest req, IAdminService adminService) =>
{
    try
    {
        var result = await adminService.UpgradeUserTierAsync(id, req);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapGet("/loans", async (IAdminService adminService) =>
{
    try
    {
        var loans = await adminService.GetAllLoansAsync();
        return Results.Ok(loans);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapPost("/loans/{id:guid}/approve", async (Guid id, ApproveLoanRequest req, IAdminService adminService) =>
{
    try
    {
        var result = await adminService.ApproveLoanAsync(id, req);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

adminGroup.MapPost("/loans/{id:guid}/reject", async (Guid id, ApproveLoanRequest req, IAdminService adminService) =>
{
    try
    {
        var result = await adminService.RejectLoanAsync(id, req);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 8. Bill Payments Endpoints
var billsGroup = app.MapGroup("/api/bills").WithTags("Bills").RequireAuthorization();

billsGroup.MapPost("/pay", async (PayBillRequest req, IBillService billService) =>
{
    try
    {
        var result = await billService.PayBillAsync(req);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 9. Fixed Savings Endpoints
var savingsGroup = app.MapGroup("/api/savings").WithTags("Savings").RequireAuthorization();

savingsGroup.MapGet("/fixed-deposits", async (ISavingsService savingsService) =>
{
    try
    {
        var deposits = await savingsService.GetUserFixedDepositsAsync();
        return Results.Ok(deposits);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

savingsGroup.MapPost("/fixed-deposits", async (CreateFixedDepositRequest req, ISavingsService savingsService) =>
{
    try
    {
        var created = await savingsService.CreateFixedDepositAsync(req);
        return Results.Created($"/api/savings/fixed-deposits/{created.Id}", created);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

savingsGroup.MapPost("/fixed-deposits/{id:guid}/liquidate", async (Guid id, ISavingsService savingsService) =>
{
    try
    {
        var result = await savingsService.LiquidateFixedDepositAsync(id);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// 10. Loans Endpoints
var loansGroup = app.MapGroup("/api/loans").WithTags("Loans").RequireAuthorization();

loansGroup.MapGet("/", async (ILoanService loanService) =>
{
    try
    {
        var loans = await loanService.GetUserLoansAsync();
        return Results.Ok(loans);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

loansGroup.MapPost("/apply", async (ApplyLoanRequest req, ILoanService loanService) =>
{
    try
    {
        var result = await loanService.ApplyLoanAsync(req);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
