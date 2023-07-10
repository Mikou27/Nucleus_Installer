using Newtonsoft.Json.Linq;

namespace Updater
{
    internal static class VersionCheck
    {
        private static string gitApi = "https://api.github.com/repos/SplitScreen-Me/splitscreenme-nucleus/";
        //private static string gitApi = "https://api.github.com/repos/Mikou27/splitscreenme-nucleus/";

        public static string CheckAppUpdate()
        {
            HttpClient http = new HttpClient();
            string response = http.Get(gitApi + "tags");

            if (response == null)
            {
                return string.Empty;
            }

            JArray versions = JArray.Parse(response);

            return versions[0]["name"].ToString();          
        }   
    }
}
