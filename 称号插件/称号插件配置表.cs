using Newtonsoft.Json;
using System.IO;
using TShockAPI;
using TShockAPI.Hooks;

namespace 称号插件
{
    public class 称号插件配置表
    {
        public static void Reload(ReloadEventArgs args)
        {
            try
            {
                Reload();
                args.Player.SendSuccessMessage($"[称号插件]重载成功！");
            }
            catch
            {
                TSPlayer.Server.SendErrorMessage($"[称号插件]配置文件读取错误");
            }
        }
        internal static HttpClient client = new();
        public static async void Reload()//重载
        {
            try
            {
                string path = 称号插件.path;
                if (!File.Exists(path))
                {
                    FileTools.CreateIfNot(Path.Combine(path), JsonConvert.SerializeObject(称号插件.配置, Formatting.Indented));
                    称号插件.配置 = JsonConvert.DeserializeObject<配置数据>(File.ReadAllText(Path.Combine(path)));
                    File.WriteAllText(path, JsonConvert.SerializeObject(称号插件.配置, Formatting.Indented));
                }
                称号插件.配置 = JsonConvert.DeserializeObject<配置数据>(File.ReadAllText(Path.Combine(path)));//正常读一遍配置文件
                File.WriteAllText(path, JsonConvert.SerializeObject(称号插件.配置, Formatting.Indented));
                if (称号插件.配置.启用远程称号)
                {
                    var responseBody = await client.GetStringAsync(称号插件.配置.远程称号接口);
                    称号插件.称号配置 = JsonConvert.DeserializeObject<称号数据>(responseBody);
                }
                else
                {
                    if (!File.Exists(称号插件.配置.称号数据路径))
                    {
                        File.WriteAllText(称号插件.配置.称号数据路径, JsonConvert.SerializeObject(称号插件.称号配置, Formatting.Indented));
                    }
                    称号插件.称号配置 = JsonConvert.DeserializeObject<称号数据>(File.ReadAllText(Path.Combine(称号插件.配置.称号数据路径)));
                }
            }
            catch (Exception ex)
            {
                TSPlayer.Server.SendErrorMessage("[称号插件]配置文件读取错误\n" + ex.ToString());
            }
        }
        public static void Write()
        {
            File.WriteAllText(称号插件.配置.称号数据路径, JsonConvert.SerializeObject(称号插件.称号配置, Formatting.Indented));
        }
        public class 配置数据
        {
            public string 说明1 = "称号设置中,前缀、名称、后缀设置为null则与玩家当前用户组的前后缀一致";
            public bool 是否开启称号插件 = true;
            public string 称号数据路径 = "tshock/称号插件数据.json";//存放玩家称号数据的地方
            public bool 启用远程称号 = false;//开启则禁用指令，因为插件无法写入数据
            public string 远程称号接口 = "http://";//读取指定链接的称号插件数据文件，下方的称号设置失效，请确保json格式正确
            public string 聊天格式 = "{前前缀}{前缀}{角色名}{后缀}{后后缀}: {聊天内容}";
            public string 聊天气泡启用时玩家名称格式 = "{前前缀}{前缀}{角色名}{后缀}{后后缀}";
            public bool 是否开启聊天气泡 = false;
            public int 称号最大字符数 = 13;
            public bool 是否允许管理绕过最大字符数检测 = true;
            public bool 是否允许无称号玩家给自己称号 = false;
        }
        public class 称号数据
        {
            public List<名称> 称号设置 = new() { };
        }
        public class 名称
        {
            public string 玩家用户名 = "";
            public string? 前前缀 = "";
            public string? 前缀 = null;
            public string? 角色名 = null;
            public string? 后缀 = null;
            public string? 后后缀 = "";
            public 颜色 颜色 = new() { };
        }
        public class 颜色
        {
            public byte R = 255;
            public byte G = 255;
            public byte B = 255;
        }
        public class 聊天信息
        {
            public string? 前前缀 = "";
            public string? 前缀 = null;
            public string? 角色名 = null;
            public string? 后缀 = null;
            public string? 后后缀 = "";
            public 颜色 颜色 = new() { };
        }
    }
}

