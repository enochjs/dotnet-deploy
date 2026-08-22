using Application.Auth;
using Application.Common;
using Domain.Entities;
using FluentValidation;

namespace Application.Users;

public sealed class UserService(
    IUserRepository users,
    IPasswordHashService passwordHashService,
    IValidator<CreateUserRequest> createValidator,
    IValidator<UpdateUserRequest> updateValidator)
{
    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken
    )
    {
        await ValidateAsync(createValidator, request, cancellationToken);
        var mobile = request.Mobile.Trim();
        var exists = await users.ExistsByMobileOrUserIdAsync(
            mobile,
            mobile,
            excludingId: null,
            cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException("USER_MOBILE_EXISTS", "手机号已存在");
        }
        
        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            UserId = mobile,
            Email = NormalizeOptional(request.Email),
            Name = request.Name.Trim(),
            Mobile = mobile,
            ManagerUserId = NormalizeOptional(request.ManagerUserId),
            Role = request.Role ?? UserRoles.Other,
            Status = request.Status ?? UserStatuses.Enabled,
            CreatedAt = now,
            UpdatedAt = now,
        };
        
        user.PasswordHash = passwordHashService.HashPassword(user, request.Password);

        users.Add(user);
        await users.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(
        int id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        
        await ValidateAsync(updateValidator, request, cancellationToken);
        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException("USER_NOT_FOUND", "user not exist");
        }

        if (!string.IsNullOrWhiteSpace(request.Mobile))
        {
            var mobile = request.Mobile.Trim();
            var exists = await users.ExistsByMobileOrUserIdAsync(
                mobile,
                mobile,
                excludingId: id,
                cancellationToken);
            if (exists)
            {
                throw new BusinessRuleException("USEr_MOBILE_EXISTS", "mobile already exists");
            }

            user.Mobile = mobile;
        }

        if (request.Email is not null)
        {
            user.Email = NormalizeOptional(request.Email);
        }

        if (request.Name is not null)
        {
            user.Name = request.Name.Trim();
        }

        if (request.Role.HasValue)
        {
            user.Role = request.Role.Value;
        }

        if (request.Status.HasValue)
        {
            user.Status = request.Status.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = passwordHashService.HashPassword(user, request.Password);
        }

        if (request.ManagerUserId is not null)
        {
            var managerUserId = NormalizeOptional(request.ManagerUserId);
            var manager = managerUserId is null
                ? null
                : await users.FindByUserIdAsync(managerUserId, cancellationToken);

            user.ManagerUserId = managerUserId;
            user.ManagerDingTalkUserId = manager?.DingTalkUserId;
        }
        
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await users.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse> GetDetailAsync(int id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException("USER_NOT_FOUND", "user not exist");
        }
        return ToResponse(user);
    }

    public async Task<IReadOnlyList<UserResponse>> SearchAsync(string key, CancellationToken cancellationToken)
    {
        var normalizedKey = key.Trim();
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            return [];
        }

        var localUsers = await users.SearchLocalAsync(normalizedKey, cancellationToken);
        return localUsers.Select(ToResponse).ToList();
    }

    public async Task<PagedResult<UserResponse>> PageAsync(
        UserQueryRequest query,
        CancellationToken cancellationToken)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var normalizeQuery = new UserQueryRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Name = NormalizeOptional(query.Name),
        };

        var (items, totalCount) = await users.PageAsync(normalizeQuery, cancellationToken);
        
        return new PagedResult<UserResponse>(
            pageIndex,
            pageSize,
            totalCount,
            items.Select(ToResponse).ToList());
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.UserId,
            user.DingTalkUserId,
            user.ManagerUserId,
            user.ManagerDingTalkUserId,
            user.Email,
            user.Name,
            user.RealName,
            user.Mobile,
            user.Role,
            UserRoles.GetName(user.Role),
            user.Status,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
        {
            return;
        }
        var errors = result.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.ErrorMessage).ToArray());
        throw new RequestValidationException(errors);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    
}