/* Shared classes can be referenced by both the Client and Server */
using System.ComponentModel.DataAnnotations;

public class AuthResponseDto
{
    public bool IsAuthSuccessful { get; set; }
    public string ErrorMessage { get; set; }
    public string Token { get; set; }
}

public class UserSignUpDto
{
    [Required(ErrorMessage = "Display Name is required.")]
    public string DisplayName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }

    public bool TermsAgreedTo { get; set; }
}

public class UserSignInDto
{
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }
}

public class UserLoginDto
{
    public string Id { get; set; }
    public string IPAddress { get; set; }
    public string UserAgentInfo { get; set; }
    public DateTime LoggedInOn { get; set; }
    public string UserId { get; set; }
    public string DisplayName { get; set; }
}

public class CreateUserLoginDto
{ 
    public string UserAgentInfo { get; set; }
}

