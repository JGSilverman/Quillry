using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quillry.Server.DataAccess;
using Quillry.Server.Domain;
using Quillry.Server.Extensions;

namespace Quillry.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginsController : ControllerBase
    {
        readonly IMapper mapper;
        readonly UserLoginRepo userLoginRepo;
        readonly UserRolesRepo rolesRepo;
        readonly UserRepo userRepo;

        public UserLoginsController(
            IMapper mapper, 
            UserLoginRepo userLoginRepo,
            UserRolesRepo rolesRepo,
            UserRepo userRepo)
        {
            this.mapper = mapper;
            this.userLoginRepo = userLoginRepo;
            this.rolesRepo = rolesRepo;
            this.userRepo = userRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogins([FromQuery] string? userId = null)
        {
            try
            {
                var roles = await this.rolesRepo.GetUserRoles(await this.userRepo.GetOneAsync(filter: x => x.Id == User.GetUserId()));
                var userIsAdmin = UserIsAdmin(roles);

                if (string.IsNullOrEmpty(userId))
                {
                    // if user is admin, return logins for all users, otherwise just themselves
                    if (userIsAdmin)
                    {
                        return Ok(this.mapper.Map<List<UserLoginDto>>(await this.userLoginRepo.GetManyAsync(includedProperties: "User")));
                    }
                    else
                    {
                        return Ok(this.mapper.Map<List<UserLoginDto>>(await this.userLoginRepo.GetManyAsync(filter: x => x.UserId == User.GetUserId(), includedProperties: "User")));
                    }
                }
                else
                {
                    // user is trying to get login info for someone else
                    if (!string.Equals(userId, User.GetUserId()) && !userIsAdmin) return Unauthorized();
                    return Ok(this.mapper.Map<List<UserLoginDto>>(await this.userLoginRepo.GetManyAsync(filter: x => x.UserId == userId, includedProperties: "User")));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateUserLoginDto dto)
        {
            try
            {
                if (dto is null) return BadRequest(nameof(dto));
                if (string.IsNullOrEmpty(dto.UserAgentInfo)) return BadRequest(nameof(dto.UserAgentInfo));

                AppUserLogin createdEntity = await this.userLoginRepo.InsertOneAsync(new AppUserLogin
                {
                    Id = Guid.NewGuid().ToString(),
                    IPAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserAgentInfo = dto.UserAgentInfo.Trim(),
                    LoggedInOn = DateTime.Now,
                    UserId = User.GetUserId(),
                });

                if (createdEntity is null) return BadRequest("Unable to create login record.");
                AppUserLogin entityToReturn = await this.userLoginRepo.GetOneAsync(filter: x => x.Id == createdEntity.Id);
                UserLoginDto dtoToReturn = this.mapper.Map<UserLoginDto>(entityToReturn);
                return Ok(dtoToReturn);
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
