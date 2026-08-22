using Domain.Entities;

namespace Application.Users;

public interface IUserRepository
{
    Task<bool> ExistsByMobileOrUserIdAsync(
        string mobile,
        string userId,
        int? excludingId,
        CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(
        int id,
        CancellationToken cancellationToken
    );

    Task<User?> FindByUserIdAsync(
        string userId,
        CancellationToken cancellationToken
    );
    
    Task<IReadOnlyList<User>> SearchLocalAsync(string key, CancellationToken cancellationToken);

    Task<(IReadOnlyList<User> items, int TotalCount)> PageAsync(
        UserQueryRequest query,
        CancellationToken cancellationToken);
    
    void Add(User user);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
}