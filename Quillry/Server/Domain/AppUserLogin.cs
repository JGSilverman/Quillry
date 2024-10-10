namespace Quillry.Server.Domain
{
    public class AppUserLogin
    {
        public string Id { get; set; }
        public string IPAddress { get; set; }
        public string UserAgentInfo { get; set; }
        public DateTime LoggedInOn { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
