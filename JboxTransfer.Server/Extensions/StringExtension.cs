namespace JboxTransfer.Server.Extensions
{
    public static class StringExtension
    {
        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            if (string.IsNullOrWhiteSpace(uri.Query))
            {
                return new Dictionary<string, string>();
            }
            //1.去除第一个前导?字符
            var dic = uri.Query.Substring(1)
                    //2.通过&划分各个参数
                    .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                    //3.通过=划分参数key和value,且保证只分割第一个=字符
                    .Select(param => param.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                    //4.通过相同的参数key进行分组
                    .GroupBy(part => part[0], part => part.Length > 1 ? part[1] : string.Empty)
                    //5.将相同key的value以,拼接
                    .ToDictionary(group => group.Key, group => string.Join(",", group));

            return dic;
        }
    }
}
