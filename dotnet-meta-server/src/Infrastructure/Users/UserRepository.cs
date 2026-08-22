using Application.Users;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Users;

public sealed class UserRepository(MetaServerDbContext dbContext): IUserRepository
{
    public Task<bool> ExistsByMobileOrUserIdAsync(string mobile, string userId, int? excludingId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AnyAsync(
                user => (excludingId == null || user.Id != excludingId.Value) &&
                        (user.Mobile == mobile || user.UserId == userId), cancellationToken);
    }
    
    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> FindByUserIdAsync(string userId, CancellationToken cancellationToken)
    { 
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> SearchLocalAsync(string key, CancellationToken cancellationToken)
    {
        var patten = $"%{key}%";
        return await dbContext.Users
            .Where(user =>
                EF.Functions.ILike(user.Name, patten)
                || (user.RealName != null && EF.Functions.ILike(user.RealName, patten))
                || user.UserId == key
            )
            .OrderByDescending(user => user.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

    }

    public async Task<(IReadOnlyList<User> items, int TotalCount)> PageAsync(UserQueryRequest query, CancellationToken cancellationToken)
    {
        var users = dbContext.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var pattern = $"%{query.Name.Trim()}%";
            users = users.Where(user => EF.Functions.ILike(user.Name, pattern));
        }
        var totalCount = await users.CountAsync(cancellationToken);
        var items = await users
            .OrderByDescending(user => user.Id)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);   
    }
}