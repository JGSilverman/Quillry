using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using Blazored.LocalStorage;
using Quillry.Client.Helpers;

namespace Quillry.Client.Services
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly JsonSerializerOptions _options;

        public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();

            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token)) return new AuthenticationState(new ClaimsPrincipal(identity));

            var tokenIsValid = CheckTokenIsValid(token);

            if (!tokenIsValid)
            {
                await SignOut();
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }

            identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public static long GetTokenExpirationTime(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
            var ticks = long.Parse(tokenExp);
            return ticks;
        }

        public static bool CheckTokenIsValid(string token)
        {
            var tokenTicks = GetTokenExpirationTime(token);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks).UtcDateTime;

            var now = DateTime.Now.ToUniversalTime();

            var valid = tokenDate >= now;

            return valid;
        }

        public async Task<string> GetToken()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token)) return String.Empty;
            return token;
        }

        public async Task<string> GetUserIdFromToken()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token)) return String.Empty;

            IEnumerable<Claim> claims = JwtParser.ParseClaimsFromJwt(token);
            return claims.FirstOrDefault(u => u.Type.Contains("nameidentifier"))?.Value;
        }

        public async Task<bool> UserHasRole(string roleName)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrWhiteSpace(token)) return false;

            IEnumerable<Claim> claims = JwtParser.ParseClaimsFromJwt(token);
            var roles = claims.FirstOrDefault(u => u.Type.Contains("roles"))?.Value;
            return true;
        }

        public async Task<AuthResponseDto> SignIn(UserSignInDto userForAuthentication)
        {
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append("api/auth/signin");

            var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), userForAuthentication);

            var authContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponseDto>(authContent, _options);

            if (!response.IsSuccessStatusCode)
                return result;

            await _localStorage.SetItemAsync("authToken", result.Token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return new AuthResponseDto { IsAuthSuccessful = true };
        }

        //public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordModel)
        //{
        //    StringBuilder urlBuiler = new StringBuilder();
        //    urlBuiler.Append("api/auth/ResetPassword");

        //    var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), resetPasswordModel);
        //    return response.IsSuccessStatusCode;
        //}
        //public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordModel)
        //{
        //    StringBuilder urlBuiler = new StringBuilder();
        //    urlBuiler.Append("api/users/ChangePassword");

        //    var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), changePasswordModel);
        //    return response.IsSuccessStatusCode;
        //}
        public async Task<bool> ConfirmEmail(string userId, string code)
        {
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/auth/ConfirmEmail?userId={userId}&code={code}");

            var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResendEmailConfirmation(string email)
        {
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/auth/ResendEmailConfirmation?email={email}");

            var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/auth/ForgotPassword?email={email}");

            var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<AuthResponseDto> SignUp(UserSignUpDto userForAuthentication)
        {
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append("api/auth/signup");

            var response = await _httpClient.PostAsJsonAsync(urlBuiler.ToString(), userForAuthentication);

            var authContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponseDto>(authContent, _options);

            if (!response.IsSuccessStatusCode)
                return result;

            await _localStorage.SetItemAsync("authToken", result.Token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return new AuthResponseDto { IsAuthSuccessful = true };
        }

        public async Task SignOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
