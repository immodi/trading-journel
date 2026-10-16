using Application.DTOs;

namespace Application.Responses.Auth;

public enum RegisterOutcome
{
    Success,
    EmailTaken,
    UsernameTaken,
    Conflict,
}

public record RegisterResult(RegisterOutcome Outcome, UserDto? User)
{
    public bool IsSuccess => Outcome == RegisterOutcome.Success;

    public static RegisterResult Success(UserDto user) =>
        new(RegisterOutcome.Success, user);

    public static RegisterResult EmailTaken() =>
        new(RegisterOutcome.EmailTaken, null);

    public static RegisterResult UsernameTaken() =>
        new(RegisterOutcome.UsernameTaken, null);

    public static RegisterResult Conflict() =>
        new(RegisterOutcome.Conflict, null);
}
