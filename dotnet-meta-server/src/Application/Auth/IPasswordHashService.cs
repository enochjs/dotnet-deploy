using Domain.Entities;

namespace Application.Auth;

public interface IPasswordHashService
{
  string HashPassword(User user, string password);
  bool VerifyPassword(User user, string password);
}