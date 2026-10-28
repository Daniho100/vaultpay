using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VaultPay.Application.Common.Interfaces;
using VaultPay.Application.DTOs.Accounts;
using VaultPay.Application.DTOs.Auth;
using VaultPay.Domain.Entities;
using VaultPay.Domain.Enums;
using VaultPay.Domain.Exceptions;

namespace VaultPay.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly IAccountService _accountService;

    public AuthService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IJwtTokenService jwtTokenService,
        ICurrentUserService currentUserService,
        IApplicationDbContext context,
        IAccountService accountService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _currentUserService = currentUserService;
        _context = context;
        _accountService = accountService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new DomainException($"Email '{request.Email}' is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true,
            Tier = request.Tier,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException($"User registration failed: {errors}");
        }

        // Assign specified role or default Customer
        var assignedRole = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer";
        if (!await _roleManager.RoleExistsAsync(assignedRole))
        {
            await _roleManager.CreateAsync(new Role(assignedRole));
        }
        await _userManager.AddToRoleAsync(user, assignedRole);

        var roles = await _userManager.GetRolesAsync(user);

        // Create initial account
        var initialAccountName = string.IsNullOrWhiteSpace(request.AccountName) ? $"{user.FullName}'s Wallet" : request.AccountName;
        var initialAccount = new Account
        {
            UserId = user.Id,
            AccountNumber = $"10{Random.Shared.Next(10000000, 99999999)}",
            AccountName = initialAccountName,
            CurrencyCode = "NGN",
            Balance = 0.00m,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        _context.Accounts.Add(initialAccount);
        await _context.SaveChangesAsync(cancellationToken);

        var (token, expires) = _jwtTokenService.GenerateToken(user, roles);

        return new AuthResponse
        {
            Token = token,
            Expiration = expires,
            Roles = roles,
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                Tier = user.Tier,
                CreatedAt = user.CreatedAt,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new DomainException("Invalid email or password.");
        }

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
        {
            throw new DomainException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expires) = _jwtTokenService.GenerateToken(user, roles);

        return new AuthResponse
        {
            Token = token,
            Expiration = expires,
            Roles = roles,
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                Tier = user.Tier,
                CreatedAt = user.CreatedAt,
                Roles = roles
            }
        };
    }

    public async Task<UserProfileDto> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException($"Change password failed: {errors}");
        }
    }

    public async Task<UserProfileDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }
}
