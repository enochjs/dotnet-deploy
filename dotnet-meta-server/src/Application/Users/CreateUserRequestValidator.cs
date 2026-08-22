using FluentValidation;

namespace Application.Users;

public sealed class CreateUserRequestValidator: AbstractValidator<CreateUserRequest>
{
    public  CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(64);
        
        RuleFor(x => x.Mobile)
            .NotEmpty()
            .Matches("^1\\d{10}$")
            .WithMessage("手机号格式不正确");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(64);
        
        RuleFor(x => x.Email)
            .MaximumLength(128)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        

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
            .When(request => !string.IsNullOrWhiteSpace(request.ManagerUserId));
    }
}