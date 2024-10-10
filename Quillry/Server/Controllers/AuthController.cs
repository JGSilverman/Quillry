using Quillry.Server.DataAccess;
using Quillry.Server.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Quillry.Server.Controllers
{
    [AllowAnonymous]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly IConfiguration config;
        readonly UserManager<AppUser> userManager;
        readonly UserRepo userRepo;

        public AuthController(
                IConfiguration config,
                UserManager<AppUser> userManager,
                UserRepo userRepo)
        {
            this.config = config;
            this.userManager = userManager;
            this.userRepo = userRepo;
        }

        [HttpPost("signup")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SignUpAsync(UserSignUpDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.Email))
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Email is required.",
                        Token = null
                    });

                var user = await this.userManager.FindByEmailAsync(dto.Email);

                if (user != null)
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Email is not available.",
                        Token = null
                    });

                if (!dto.TermsAgreedTo)
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Terms and Conditions are required.",
                        Token = null
                    });

                var userWithDisplayName = await this.userRepo.GetOneAsync(filter: filter => filter.DisplayName == dto.DisplayName);
                if (userWithDisplayName != null)
                    return BadRequest(new AuthResponseDto
                    {
                        IsAuthSuccessful = false,
                        ErrorMessage = "Display name is not available.",
                        Token = null
                    });

                user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = dto.Email.Trim(),
                    UserName = dto.Email.Trim(),
                    DisplayName = dto.DisplayName.Trim(),
                    TermsAgreedTo = dto.TermsAgreedTo,
                    TermsAgreedToOn = DateTime.Now,
                    PasswordLastChanged = DateTime.Now,
                    JoinedOn = DateTime.Now,
                };

                var userCreateResult = await this.userManager.CreateAsync(user, dto.Password);
                if (userCreateResult.Succeeded)
                {

                    var signingCredentials = GetSigningCredentials();
                    var claims = await GetClaims(user);
                    var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
                    var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                    var code = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    StringBuilder urlBuilder = new StringBuilder();

                    urlBuilder.Append("https://quillry.com/account/confirmemail?");
                    urlBuilder.Append($"userId={user.Id}&code={code}");
                    var callbackUrl = urlBuilder.ToString();

                    //await this.emailService.SendEmailAsync(
                    //    fromEmail: "noreply@quillry.com",
                    //    toEmail: user.Email,
                    //    subject: "Please confirm your e-mail address",
                    //    message: $"Hey {user.DisplayName}. To get up and running on Quillry, you’ll just need to click <a class='btn btn-primary btn-lg' href='{callbackUrl}'>here</a> to confirm your email address.");

                    //await this.emailService.SendEmailAsync(
                    //    fromEmail: "noreply@quillry.com",
                    //    toEmail: "joshuagsilverman@gmail.com",
                    //    subject: "New Quillry User Registered",
                    //    message: $"{user.DisplayName} just signed up.");

                    return Ok(new AuthResponseDto
                    {
                        IsAuthSuccessful = true,
                        Token = token
                    });
                }

                return BadRequest(new AuthResponseDto
                {
                    IsAuthSuccessful = false,
                    Token = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("signin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignInAsync([FromBody] UserSignInDto userLoginResource)
        {
            var user = this.userManager.Users.SingleOrDefault(u => u.UserName == userLoginResource.Email.Trim());
            if (user is null)
                return NotFound(new AuthResponseDto
                {
                    IsAuthSuccessful = false,
                    ErrorMessage = "User not found"
                });

            if (user.LockoutEnabled && user.LockoutEnd is not null && user.LockoutEnd.Value.Date != DateTime.MinValue)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsAuthSuccessful = false,
                    ErrorMessage = "User is locked out."
                });
            }

            var userSigninResult = await this.userManager.CheckPasswordAsync(user, userLoginResource.Password);
            if (userSigninResult)
            {
                var signingCredentials = GetSigningCredentials();
                var claims = await GetClaims(user);
                var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                return Ok(new AuthResponseDto
                {
                    IsAuthSuccessful = true,
                    Token = token
                });
            }

            return BadRequest(new AuthResponseDto
            {
                IsAuthSuccessful = false,
                ErrorMessage = "Email or password is incorrect."
            });
        }

        SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = this.config.GetSection("JWTSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["securityKey"]);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        async Task<List<Claim>> GetClaims(AppUser user)
        {
            var roles = await this.userManager.GetRolesAsync(user);
            //UserImage image = await this.userImageRepo.GetOneAsync(filter: x => x.UserId == user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //if (image is not null && image.Id > 0)
            //{
            //    claims.Add(new Claim(ClaimTypes.UserData, image.Url));
            //}

            return claims;
        }

        JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = this.config.GetSection("JWTSettings");
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings["validIssuer"],
                audience: jwtSettings["validAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expiryInMinutes"])),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }
    }
}
