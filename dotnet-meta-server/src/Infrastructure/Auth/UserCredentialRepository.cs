using Application.Auth;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth;

public sealed class UserCredentialRepository(MetaServerDbContext dbContext) : IUserCredentialRepository
{
  private readonly MetaServerDbContext _dbContext = dbContext;
  
  public Task<User?> FindByAccountAsync(string account, CancellationToken cancellationToken)
  {
    return _dbContext.Users
      .AsNoTracking()
      .SingleOrDefaultAsync(user => user.UserId == account || user.Mobile == account, cancellationToken);
  }

  public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken)
  {
    return _dbContext.Users
      .AsNoTracking()
      .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
  }

}
