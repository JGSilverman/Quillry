using AutoMapper;
using Quillry.Server.DataAccess;
using Quillry.Server.Domain;
using Quillry.Server.Extensions;
using Quillry.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Quillry.Server.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        readonly IConfiguration config;
        readonly UserManager<AppUser> userManager;
        readonly UserRepo userRepo;
        readonly UserRolesRepo rolesRepo;
        readonly IMapper mapper;

        public UsersController(
                IConfiguration config,
                UserManager<AppUser> userManager,
                UserRepo userRepo,
                UserRolesRepo rolesRepo,
                IMapper mapper)
        {
            this.config = config;
            this.userManager = userManager;
            this.userRepo = userRepo;
            this.rolesRepo = rolesRepo;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var roles = await this.rolesRepo.GetUserRoles(await this.userRepo.GetOneAsync(filter: x => x.Id == User.GetUserId()));
                var userIsAdmin = UserIsAdmin(roles);
                if (!userIsAdmin) return Unauthorized();
                return Ok(this.mapper.Map<List<UserAccountDto>>(await this.userRepo.GetManyAsync()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UserAccountDto userAccount)
        {
            try
            {
                AppUser dataToUpdate = await this.userRepo.GetOneAsync(filter: x => x.Id == userAccount.Id);
                if (dataToUpdate is null || string.IsNullOrEmpty(dataToUpdate.Id)) return NotFound();

                if (!string.Equals(userAccount.Id, User.GetUserId()))
                {
                    var roles = await this.rolesRepo.GetUserRoles(await this.userRepo.GetOneAsync(filter: x => x.Id == User.GetUserId()));
                    var userIsAdmin = UserIsAdmin(roles);
                    if (!userIsAdmin) return Unauthorized();
                }

                dataToUpdate.Email = userAccount.Email;
                dataToUpdate.NormalizedEmail = userAccount.Email.ToUpper();
                dataToUpdate.EmailConfirmed = userAccount.EmailConfirmed;
                dataToUpdate.PhoneNumberConfirmed = userAccount.PhoneNumberConfirmed;
                dataToUpdate.PhoneNumber = userAccount.PhoneNumber;
                dataToUpdate.TermsAgreedTo = userAccount.TermsAgreedTo;
                dataToUpdate.TermsAgreedToOn = userAccount.TermsAgreedToOn;
                dataToUpdate.LockoutEnabled = userAccount.LockoutEnabled;
                dataToUpdate.LockoutEnd = userAccount.LockoutEnd;

                bool updated = await this.userRepo.UpdateOneAsync(dataToUpdate);

                if (!updated) return BadRequest("Unable to update data");
                AppUser entityToReturn = await this.userRepo.GetOneAsync(filter: x => x.Id == dataToUpdate.Id);
                UserAccountDto dtoToReturn = this.mapper.Map<UserAccountDto>(entityToReturn);
                return Ok(dtoToReturn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmEmailAsync(string code)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Code is required");

            try
            {
                var user = await this.userManager.FindByIdAsync(User.GetUserId());
                if (user is null) return NotFound();

                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                var result = await this.userManager.ConfirmEmailAsync(user, code);

                return result.Succeeded ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        bool UserIsAdmin(IList<string> roles)
        {
            if (roles is null) return false;
            if (!roles.Any()) return false;
            return roles.Contains("Admin");
        }
    }
}
