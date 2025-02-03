namespace JboxTransfer.Server.Helpers
{
    public static class HttpHelper
    {
        public static Dictionary<string, string> GetCommonHeaders()
        {
            var headers = new Dictionary<string, string>();
            headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            headers.Add("accept-encoding", "gzip, deflate, br");
            headers.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            headers.Add("cache-control", "max-age=0");
            headers.Add("sec-ch-ua", "\"Chromium\";v=\"110\", \"Not A(Brand\";v=\"24\", \"Microsoft Edge\";v=\"110\"");
            headers.Add("sec-ch-ua-mobile", "?0");
            headers.Add("sec-ch-ua-platform", "\"Linux\"");
            headers.Add("sec-fetch-dest", "document");
            headers.Add("sec-fetch-mode", "navigate");
            headers.Add("sec-fetch-site", "none");
            headers.Add("sec-fetch-user", "?1");
            headers.Add("upgrade-insecure-requests", "1");
            headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.0.0");

            return headers;
        }
    }
}
