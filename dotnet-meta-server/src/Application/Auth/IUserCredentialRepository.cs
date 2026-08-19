using Domain.Entities;

namespace Application.Auth;

public interface IUserCredentialRepository
{
  Task<User?> FindByAccountAsync(string account, CancellationToken cancellationToken);
  Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken);
}