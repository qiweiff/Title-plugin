using Newtonsoft.Json;

namespace 称号插件
{
    public class 称号插件配置表
    {
        public static 称号插件配置表 GetConfig()
        {
            var config = new 称号插件配置表();
            if (!File.Exists(path))
            {
                using StreamWriter wr = new(path);
                wr.WriteLine(JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            else
            {
                using StreamReader re = new(path);
                config = JsonConvert.DeserializeObject<称号插件配置表>(re.ReadToEnd());
            }
            return config;
        }

        public string 说明 = "称号设置中，前缀、名称、后缀设置为null则与玩家当前用户组的前后缀一致";
        public bool 是否开启称号插件 = true;
        public List<名称> 称号设置 = new() { new 名称() };
        private const string path = "tshock/称号插件配置表.json";
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

