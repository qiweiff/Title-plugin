using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace 称号插件
{
    [ApiVersion(2, 1)]//api版本
    public class 称号插件 : TerrariaPlugin
    {
        /// 插件作者
        public override string Author => "奇威复反";
        /// 插件说明
        public override string Description => "可高自由给玩家设置称号的称号插件";
        /// 插件名字
        public override string Name => "称号插件";
        /// 插件版本
        public override Version Version => new(1, 1, 0, 0);
        /// 插件处理
        public 称号插件(Main game) : base(game)
        {
        }
        //插件启动时，用于初始化各种狗子
        public 称号插件配置表 配置;
        public string path = "tshock/称号插件配置表.json";
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);//钩住游戏初始化时
            ServerApi.Hooks.ServerChat.Register(this, 聊天);
            称号插件配置表.GetConfig();
            Reload();
        }
        /// 插件关闭时
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);//销毁游戏初始化狗子
                ServerApi.Hooks.ServerChat.Deregister(this, 聊天);

            }
            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)//游戏初始化的狗子
        {
            //第一个是权限，第二个是子程序，第三个是指令
            Commands.ChatCommands.Add(new Command("称号", 指令1, "称号", "给称号", "ch") { });
            Commands.ChatCommands.Add(new Command("称号", 重载, "reload", "称号重载") { });
            Commands.ChatCommands.Add(new Command("改称号", 指令2, "改称号", "gch") { });


        }

        private void 指令1(CommandArgs args)
        {
            try
            {
                if (args.Player.Group.Name != TShock.Config.Settings.DefaultGuestGroupName) { }
                else
                {
                    args.Player.SendInfoMessage($"请先登录");
                    return;
                }
                string? 玩家用户名 = args.Parameters[0];
                string? 称号位置 = args.Parameters[1];
                string? 称号名 = "";
                if (称号位置 != "sc" && 称号位置 != "删除")
                {
                    int max = args.Parameters.Count - 2;
                    称号名 = string.Join(" ", args.Parameters.GetRange(2, max));
                }
                if (称号名.Length <= 配置.称号最大字符数)
                {
                    设置称号(args, 玩家用户名, 称号位置, 称号名);//这样便于修改
                }
                else
                {
                    if (配置.是否允许管理绕过最大字符数检测)
                    {
                        设置称号(args, 玩家用户名, 称号位置, 称号名);
                    }
                    else
                    {
                        args.Player.SendInfoMessage($"设置的称号过长！");
                    }
                }
            }
            catch
            {
                args.Player.SendInfoMessage($"设置指令:/称号 玩家名 称号位置 称号名");
                args.Player.SendInfoMessage($"删除指令:/称号 玩家名 删除 ");
                args.Player.SendInfoMessage($"称号位置:前前缀、前缀、角色名、后缀、后后缀");
                args.Player.SendInfoMessage($"称号设置中，前缀、名称、后缀设置为null则与玩家当前用户组的前后缀一致");
                return;
            }
        }

        private void 指令2(CommandArgs args)
        {
            try
            {
                if (args.Player.Group.Name != TShock.Config.Settings.DefaultGuestGroupName) { }
                else
                {
                    args.Player.SendInfoMessage($"请先登录");
                    return;
                }
                if (配置.称号设置.Exists(a => a.玩家用户名 == args.Player.Name) || 配置.是否允许无称号玩家给自己称号) { }
                else
                {
                    args.Player.SendInfoMessage($"您还没有称号。");
                    return;
                }
                try
                {
                    string? 玩家用户名 = args.Player.Name;
                    string? 称号位置 = args.Parameters[0];
                    string? 称号名 = "";
                    if (称号位置 != "sc" && 称号位置 != "删除" || 配置.是否允许无称号玩家给自己称号)
                    {
                        int max = args.Parameters.Count - 1;
                        称号名 = string.Join(" ", args.Parameters.GetRange(1, max));
                    }
                    else
                    {
                        args.Player.SendInfoMessage($"我不推荐你这样做。");
                        return;
                    }
                    if (称号名.Length <= 配置.称号最大字符数)
                    {
                        设置称号(args, 玩家用户名, 称号位置, 称号名);
                    }
                    else
                    {
                        if (配置.是否允许管理绕过最大字符数检测 && args.Player.HasPermission("称号"))
                        {
                            设置称号(args, 玩家用户名, 称号位置, 称号名);
                        }
                        else
                        {
                            args.Player.SendInfoMessage($"设置的称号过长！");
                        }
                    }

                }
                catch
                {
                    args.Player.SendInfoMessage($"修改指令:/称号 称号位置 称号名");
                    args.Player.SendInfoMessage($"称号位置:前前缀、前缀、角色名、后缀、后后缀");
                    args.Player.SendInfoMessage($"用于修改自己的称号");
                    return;
                }


            }
            catch
            {
                args.Player.SendErrorMessage($"[称号插件]发生错误！");
            }
        }
        public void 设置称号(CommandArgs args, string? 玩家用户名, string? 称号位置, string? 称号名)
        {
            string? 前前缀 = "";
            string? 前缀 = null;
            string? 角色名 = null;
            string? 后缀 = null;
            string? 后后缀 = "";
            if (配置.称号设置.Exists(a => a.玩家用户名 == 玩家用户名))
            {
                var z = 配置.称号设置.Find(s => s.玩家用户名 == 玩家用户名);
                前前缀 = z.前前缀;
                前缀 = z.前缀;
                角色名 = z.角色名;
                后缀 = z.后缀;
                后后缀 = z.后后缀;
                switch (称号位置)
                {
                    case "前前缀":
                    case "qqz":
                        前前缀 = 称号名;
                        break;
                    case "前缀":
                    case "qz":
                        前缀 = 称号名;
                        break;
                    case "角色名":
                    case "角色":
                    case "jsm":
                    case "js":
                        角色名 = 称号名;
                        break;
                    case "后缀":
                    case "hz":
                        后缀 = 称号名;
                        break;
                    case "后后缀":
                    case "hhz":
                        后后缀 = 称号名;
                        break;
                    case "删除":
                    case "sc":
                        Remove(玩家用户名);
                        args.Player.SendInfoMessage($"已删除玩家：{玩家用户名}的称号");
                        return;
                    default:
                        args.Player.SendInfoMessage($"设置指令:/称号 玩家名 称号位置 称号名");
                        args.Player.SendInfoMessage($"删除指令:/称号 玩家名 删除");
                        args.Player.SendInfoMessage($"称号位置:前前缀、前缀、角色名、后缀、后后缀");
                        return;
                }
                Remove(玩家用户名);
                Add(玩家用户名, 前前缀, 前缀, 角色名, 后缀, 后后缀);
                args.Player.SendInfoMessage($"设置成功\n{玩家用户名}的称号为:\n前前缀：{前前缀}\n前缀：{前缀}\n角色名：{角色名}\n后缀：{后缀}\n后后缀：{后后缀}");
            }
            else
            {
                switch (称号位置)
                {
                    case "前前缀":
                    case "qqz":
                        前前缀 = 称号名;

                        break;
                    case "前缀":
                    case "qz":
                        前缀 = 称号名;
                        break;
                    case "角色名":
                    case "角色":
                    case "jsm":
                    case "js":
                        角色名 = 称号名;
                        break;
                    case "后缀":
                    case "hz":
                        后缀 = 称号名;
                        break;
                    case "后后缀":
                    case "hhz":
                        后后缀 = 称号名;
                        break;
                    case "删除":
                    case "sc":
                        args.Player.SendInfoMessage($"玩家：{玩家用户名}没有称号");
                        return;
                    default:
                        args.Player.SendInfoMessage($"设置指令:/称号 玩家名 称号位置 称号名");
                        args.Player.SendInfoMessage($"删除指令:/称号 玩家名 删除");
                        args.Player.SendInfoMessage($"称号位置:前前缀、前缀、角色名、后缀、后后缀");
                        return;
                }
                Add(玩家用户名, 前前缀, 前缀, 角色名, 后缀, 后后缀);
                args.Player.SendInfoMessage($"设置成功\n{玩家用户名}的称号为:\n前前缀：{前前缀}\n前缀：{前缀}\n角色名：{角色名}\n后缀：{后缀}\n后后缀：{后后缀}");
            }
        }
        private void 重载(CommandArgs args)
        {
            Reload();
            args.Player.SendErrorMessage($"[称号插件]重载成功！");
        }

        public void Remove(string? 玩家用户名)
        {
            配置.称号设置.RemoveAll(s => (s.玩家用户名 == 玩家用户名));
            File.WriteAllText(path, JsonConvert.SerializeObject(配置, Formatting.Indented));
        }
        public void Add(string? 玩家用户名, string? 前前缀, string? 前缀, string? 角色名, string? 后缀, string? 后后缀)
        {
            配置.称号设置.Add(new 名称() { 玩家用户名 = 玩家用户名, 前前缀 = 前前缀, 前缀 = 前缀, 角色名 = 角色名, 后缀 = 后缀, 后后缀 = 后后缀 });
            File.WriteAllText(path, JsonConvert.SerializeObject(配置, Formatting.Indented));
        }
        public void Reload()
        {
            配置 = JsonConvert.DeserializeObject<称号插件配置表>(File.ReadAllText(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(配置, Formatting.Indented));
        }
        private void 聊天(ServerChatEventArgs _聊天内容)
        {
            try
            {
                if (配置.是否开启称号插件 == true && TShock.Players[_聊天内容.Who].Group.Name != TShock.Config.Settings.DefaultGuestGroupName && _聊天内容.Text[..1] != TShock.Config.Settings.CommandSpecifier && _聊天内容.Text[..1] != TShock.Config.Settings.CommandSilentSpecifier)
                {

                    var z = 配置.称号设置.Find(s => s.玩家用户名 == TShock.Players[_聊天内容.Who].Name);
                    if (z is not null)
                        if (TShock.Players[_聊天内容.Who].Name == z.玩家用户名)
                        {
                            string 前缀;
                            string 角色名;
                            string 后缀;

                            if (z.前缀 == null)
                            {
                                前缀 = TShock.Players[_聊天内容.Who].Group.Prefix;
                            }
                            else
                            {
                                前缀 = z.前缀;
                            }
                            if (z.角色名 == null)
                            {
                                角色名 = TShock.Players[_聊天内容.Who].Name;
                            }
                            else
                            {
                                角色名 = z.角色名;
                            }
                            if (z.后缀 == null)
                            {
                                后缀 = TShock.Players[_聊天内容.Who].Group.Suffix;
                            }
                            else
                            {
                                后缀 = z.后缀;
                            }
                            string text = z.前前缀 + 前缀 + 角色名 + 后缀 + z.后后缀 + "：" + _聊天内容.Text;
                            TSPlayer.All.SendMessage(text, 255, 255, 255);
                            TSPlayer.Server.SendMessage(text, 255, 255, 255);//经过实践，[c/FFFFFF:],[i:]仍然生效
                            _聊天内容.Handled = true;
                        }
                }
            }
            catch
            {
                TSPlayer.Server.SendErrorMessage($"[称号插件]配置文件读取错误！！");
            }
        }
    }
}