using FluentValidation;

namespace Application.Users;

public sealed class UpdateUserRequestValidator: AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(64)
            .When(x => x.Name is not null);
        
        RuleFor(request => request.Mobile)
            .Matches("^1\\d{10}$")
            .When(request => !string.IsNullOrWhiteSpace(request.Mobile))
            .WithMessage("手机号格式不正确");
        
        RuleFor(request => request.Password)
            .MinimumLength(6)
            .MaximumLength(64)
            .When(request => !string.IsNullOrWhiteSpace(request.Password));

        RuleFor(request => request.Email)
            .MaximumLength(128)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Role)
            .Must(UserRoles.IsValid)
            .When(request => request.Role.HasValue)
            .WithMessage("角色不正确");

        RuleFor(request => request.Status)
            .Must(UserStatuses.IsValid)
            .When(request => request.Status.HasValue)
            .WithMessage("状态不正确");

        RuleFor(request => request.ManagerUserId)
            .MaximumLength(64)
            .When(request => request.ManagerUserId is not null);
    }
}