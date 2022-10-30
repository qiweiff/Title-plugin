using Newtonsoft.Json;
using TShockAPI;

namespace 称号插件
{
    public class 称号插件配置表
    {
        public static 称号插件配置表 GetConfig()
        {
            try
            {
                var config = new 称号插件配置表();
                if (!File.Exists(path))
                {
                    using StreamWriter wr = new(path);
                    wr.WriteLine(JsonConvert.SerializeObject(config, Formatting.Indented));
                }
                else
                {
                    config = JsonConvert.DeserializeObject<称号插件配置表>(File.ReadAllText(path));
                }
                return config;
            }
            catch
            {
                TSPlayer.Server.SendErrorMessage($"[称号插件]配置文件读取错误！！！");
                return new 称号插件配置表();
            }
        }
        public string 说明1 = "称号设置中，前缀、名称、后缀设置为null则与玩家当前用户组的前后缀一致";
        public string 说明2 = "给称号指令:/称号 . 重载指令:/reload . 权限:“称号”";
        public string 说明3 = "改称号指令:/改称号  . 权限:“改称号”用于改自己的称号（需要先有一个称号）,可以给权限以允许普通玩家使用";
        public string 说明4 = "一个汉字等于2字符,已设置的称号不受影响,用于防止普通玩家将自己的称号修改过长";
        public bool 是否开启称号插件 { get; set; } = true;
        public int 称号最大字符数 { get; set; } = 13;
        public bool 是否允许管理绕过最大字符数检测 { get; set; } = true;
        public bool 是否允许无称号玩家给自己称号 { get; set; } = false;
        public List<名称> 称号设置 { get; set; } = new() { };
        public static string path = "tshock/称号插件配置表.json";
    }

    public class 名称
    {
        public string? 玩家用户名 { get; set; } = "";
        public string? 前前缀 { get; set; } = "";
        public string? 前缀 { get; set; } = null;
        public string? 角色名 { get; set; } = null;
        public string? 后缀 { get; set; } = null;
        public string? 后后缀 { get; set; } = "";

    }

}

