using FluentValidation;

namespace ZasNet.Application.UseCases.Commands.Users.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.RoleId).NotEqual(0);
        RuleFor(x => x.Login).NotEmpty().MinimumLength(3).MaximumLength(10);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(15);
    }
}
