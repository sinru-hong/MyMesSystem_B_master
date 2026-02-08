namespace MyMesSystem_B.Helpers
{
    using System.Runtime.InteropServices;

    public class NetworkConnection
    {
        // 呼叫 Windows API
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

        [StructLayout(LayoutKind.Sequential)]
        public class NetResource
        {
            public int Scope;
            public int Type;
            public int DisplayType;
            public int Usage;
            public string LocalName;
            public string RemoteName; // 遠端路徑，例如 \\192.168.1.100\Shared
            public string Comment;
            public string Provider;
        }

        public static bool Connect(string remotePath, string username, string password)
        {
            var nr = new NetResource
            {
                Type = 1, // RESOURCETYPE_DISK
                RemoteName = remotePath
            };

            // 💡 執行登入動作
            int result = WNetAddConnection2(nr, password, username, 0);
            return result == 0; // 0 代表成功
        }
    }
}
