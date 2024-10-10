using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Quillry.Server.DataAccess;
using Quillry.Server.Domain;
using Quillry.Server.Extensions;
using Quillry.Server.Services;
using Quillry.Shared;

namespace Quillry.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        readonly IConfiguration config;
        readonly UserManager<AppUser> userManager;
        readonly UserRepo userRepo;
        readonly UserRolesRepo rolesRepo;
        readonly IMapper mapper;
        readonly IEmailService emailService;

        public AccountController(
                IConfiguration config,
                UserManager<AppUser> userManager,
                UserRepo userRepo,
                UserRolesRepo rolesRepo,
                IMapper mapper,
                IEmailService emailService)
        {
            this.config = config;
            this.userManager = userManager;
            this.userRepo = userRepo;
            this.rolesRepo = rolesRepo;
            this.mapper = mapper;
            this.emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountAsync()
        {
            try
            {
                AppUser data = await this.userRepo.GetOneAsync(filter: x => x.Id == User.GetUserId());
                if (data is null) return NotFound();
                return Ok(this.mapper.Map<UserAccountDto>(data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("changepassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto is null) return BadRequest(nameof(dto));
            if (string.IsNullOrEmpty(dto.CurrentPassword)) return BadRequest(nameof(dto.CurrentPassword));
            if (string.IsNullOrEmpty(dto.NewPassword)) return BadRequest(nameof(dto.NewPassword));
            if (string.IsNullOrEmpty(dto.ConfirmPassword)) return BadRequest(nameof(dto.ConfirmPassword));  
            if (!dto.PasswordsMatch) return BadRequest(nameof(dto.PasswordsMatch));

            char[] SpecialChars = "!@#$%^&*()".ToCharArray();

            if (!dto.NewPassword.Any(char.IsDigit))
            {
                return BadRequest("Password needs at least one number.");
            }

            if (!dto.NewPassword.Any(char.IsUpper))
            {
                return BadRequest("Password needs at least one uppercase letter.");
            }

            int indexOf = dto.NewPassword.IndexOfAny(SpecialChars);
            if (indexOf == -1)
            {
                return BadRequest("Password needs at least one special character.");
            }

            try
            {
                AppUser user = await userManager.FindByIdAsync(User.GetUserId());
                if (user is null) return NotFound();

                var passwordChangedResult = await userManager.ChangePasswordAsync(
                                        user, 
                                        currentPassword: dto.CurrentPassword, 
                                        newPassword: dto.NewPassword);

                if (!passwordChangedResult.Succeeded) return BadRequest("Unable to change password");

                user.PasswordLastChanged = DateTime.Now;
                await userRepo.UpdateOneAsync(user);

                await this.emailService.SendEmailAsync(
                       fromEmail: "noreply@quillry.com",
                       toEmail: user.Email,
                       subject: "Your password has changed!",
                       message: $"Hey {user.DisplayName}. Your password was just changed. If you didn't take this action, please contact us immediately.");

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
