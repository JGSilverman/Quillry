namespace Quillry.Client.Helpers
{
    public class QueryHelper
    {
        public string GetQueryParam(string paramName, string url)
        {
            var uriBuilder = new UriBuilder(url);
            var q = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            return q[paramName] ?? "";
        }
    }
}
