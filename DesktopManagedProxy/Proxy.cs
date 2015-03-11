using System;

namespace NSightProxyDLL
{
    public class Proxy
    {
        public static int Start(string path)
        {
            return AppDomain.CurrentDomain.ExecuteAssembly(path);
        }
    }
}