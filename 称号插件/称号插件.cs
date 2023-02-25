using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static Terraria.WorldBuilding.GenCondition;

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
        public override Version Version => new(1, 6, 0, 0);
        /// 插件处理
        public 称号插件(Main game) : base(game)
        {
            Order = 0;//设置插件优先级为0,tshock为0,数字越大越优先,设置为0后,插件优先及应高于tshock
        }
        public static 称号插件配置表.配置数据 配置 = new();//配置表信息
        public static 称号插件配置表.称号数据 称号配置 = new();//玩家称号信息
        public static Dictionary<string, 称号插件配置表.聊天信息> 称号信息 = new();//玩家空白称号信息，用于其他插件给玩家设置前缀、后缀、等等
        /*
         * 
         *    其他插件修改称号示例。
         *    *
         *          称号插件.称号插件.称号信息[plr.Name].前前缀 = z.前缀;
         *          称号插件.称号插件.称号信息[plr.Name].后后缀 = z.后缀;
         *    *
         *    修改“称号信息”中的称号，不影响配置表
         * 
         * 
         */
        internal static string path = "tshock/称号插件配置表.json";//配置文件路径
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.ServerChat.Register(this, 聊天);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            GeneralHooks.ReloadEvent += new GeneralHooks.ReloadEventD(称号插件配置表.Reload);
            称号插件配置表.Reload();
        }
        /// 插件关闭时
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.ServerChat.Deregister(this, 聊天);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                GeneralHooks.ReloadEvent -= new GeneralHooks.ReloadEventD(称号插件配置表.Reload);
            }
            base.Dispose(disposing);
        }
        private static void OnLeave(LeaveEventArgs args)
        {
            var plr = TShock.Players[args.Who];
            if (plr != null)
                if (称号信息.ContainsKey(plr.Name))//如果有该玩家信息，则删除
                {
                    称号信息.Remove(plr.Name);
                }
        }
        private static void OnJoin(GreetPlayerEventArgs args)
        {
            var plr = TShock.Players[args.Who];
            if (plr != null)
                if (!称号信息.ContainsKey(plr.Name))//如果没有该玩家信息，则添加
                {
                    称号信息.Add(plr.Name, new 称号插件配置表.聊天信息() { 前前缀 = "", 前缀 = null, 角色名 = null, 后缀 = null, 后后缀 = "" });
                }
        }
        #region 指令
        private void OnInitialize(EventArgs args)//游戏初始化的狗子
        {
            //第一个是权限，第二个是子程序，第三个是指令
            Commands.ChatCommands.Add(new Command("称号", 指令1, "称号", "给称号", "ch") { });
            Commands.ChatCommands.Add(new Command("改称号", 指令2, "改称号", "gch") { });
        }
        private static void 指令1(CommandArgs args)
        {
            try
            {
                if (配置.启用远程称号)
                {
                    args.Player.SendInfoMessage($"远程配置开启中，该指令失效！");
                    return;
                }
                var plr = args.Player;
                string? 玩家用户名 = args.Parameters[0];
                string? 称号名 = "";
                if (args.Parameters.Count == 1)
                {
                    查称号(args, 玩家用户名);
                    return;
                }
                string? 称号位置 = args.Parameters[1];
                if (args.Parameters[1] != "sc" && args.Parameters[1] != "删除" && args.Parameters[1] != "qqz" &&
                    args.Parameters[1] != "前前缀" && args.Parameters[1] != "qz" && args.Parameters[1] != "前缀" &&
                    args.Parameters[1] != "js" && args.Parameters[1] != "角色名" && args.Parameters[1] != "jsm" && args.Parameters[1] != "角色"
                    && args.Parameters[1] != "hz" && args.Parameters[1] != "后缀" && args.Parameters[1] != "" && args.Parameters[1] != "后后缀"
                    && args.Parameters[1] != "R" && args.Parameters[1] != "G" && args.Parameters[1] != "B"
                    )
                {

                    int max = args.Parameters.Count - 1;
                    称号名 = string.Join(" ", args.Parameters.GetRange(1, max));
                    if (称号名.Length <= 配置.称号最大字符数)
                    {
                        设置称号(plr, 玩家用户名, "前前缀", 称号名);
                    }
                    else
                    {
                        if (配置.是否允许管理绕过最大字符数检测)
                        {
                            设置称号(plr, 玩家用户名, "前前缀", 称号名);
                        }
                        else
                        {
                            plr.SendInfoMessage($"设置的称号过长！");
                        }
                    }
                    return;
                }
                if (称号位置 != "sc" && 称号位置 != "删除")
                {
                    int max = args.Parameters.Count - 2;
                    称号名 = string.Join(" ", args.Parameters.GetRange(2, max));
                }
                if (称号名.Length <= 配置.称号最大字符数)
                {
                    设置称号(plr, 玩家用户名, 称号位置, 称号名);
                }
                else
                {
                    if (配置.是否允许管理绕过最大字符数检测)
                    {
                        设置称号(plr, 玩家用户名, 称号位置, 称号名);
                    }
                    else
                    {
                        plr.SendInfoMessage($"设置的称号过长！");
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

        private static void 指令2(CommandArgs args)
        {
            if (配置.启用远程称号)
            {
                args.Player.SendInfoMessage($"远程配置开启中，该指令失效！");
                return;
            }
            if (!称号配置.称号设置.Exists(a => a.玩家用户名 == args.Player.Name) && !配置.是否允许无称号玩家给自己称号)
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
                    称号名 = string.Join(" ", args.Parameters.GetRange(1, max));//将指令后面的部分合起来
                }
                else
                {
                    args.Player.SendInfoMessage($"我不推荐你这样做。");
                    return;
                }
                if (称号名.Length <= 配置.称号最大字符数)
                {
                    设置称号(args.Player, 玩家用户名, 称号位置, 称号名);
                }
                else
                {
                    if (配置.是否允许管理绕过最大字符数检测 && args.Player.HasPermission("称号"))
                    {
                        设置称号(args.Player, 玩家用户名, 称号位置, 称号名);
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
        public static void 查称号(CommandArgs args, string? 玩家用户名)
        {
            if (称号配置.称号设置.Exists(a => a.玩家用户名 == 玩家用户名))
            {
                string? 前前缀 = "";
                string? 前缀 = null;
                string? 角色名 = null;
                string? 后缀 = null;
                string? 后后缀 = "";
                var z = 称号配置.称号设置.Find(s => s.玩家用户名 == 玩家用户名);
                if (z == null)
                {
                    args.Player.SendInfoMessage($"{玩家用户名}没有称号。");
                    return;
                }
                前前缀 = z.前前缀;
                后后缀 = z.后后缀;
                前缀 = z.前缀;
                角色名 = z.角色名;
                后缀 = z.后缀;
                args.Player.SendInfoMessage($"{玩家用户名}的称号为:\n前前缀：{前前缀}\n前缀：{前缀}\n角色名：{角色名}\n后缀：{后缀}\n后后缀：{后后缀}\nRGB：{z.颜色.R + "," + z.颜色.G + "," + z.颜色.B}");
            }
            else
            {
                args.Player.SendInfoMessage($"{玩家用户名}没有称号。");
            }
        }
        public static void 设置称号(TSPlayer plr, string 玩家用户名, string? 称号位置, string? 称号名)
        {
            string? 前前缀 = "";
            string? 前缀 = null;
            string? 角色名 = null;
            string? 后缀 = null;
            string? 后后缀 = "";
            byte R = 255;
            byte G = 255;
            byte B = 255;
            var z = 称号配置.称号设置.Find(s => s.玩家用户名 == 玩家用户名);
            if (z != null)
            {
                前前缀 = z.前前缀;
                前缀 = z.前缀;
                角色名 = z.角色名;
                后缀 = z.后缀;
                后后缀 = z.后后缀;
                R = z.颜色.R;
                G = z.颜色.G;
                B = z.颜色.B;
            }
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
                case "R":
                    if (byte.TryParse(称号名, out byte b))
                    {
                        R = b;
                    }
                    else
                    {
                        plr.SendErrorMessage("错误的颜色!");
                    }
                    break;
                case "G":
                    if (byte.TryParse(称号名, out b))
                    {
                        G = b;
                    }
                    else
                    {
                        plr.SendErrorMessage("错误的颜色!");
                    }
                    break;
                case "B":
                    if (byte.TryParse(称号名, out b))
                    {
                        B = b;
                    }
                    else
                    {
                        plr.SendErrorMessage("错误的颜色!");
                    }
                    break;
                case "删除":
                case "sc":
                    Remove(玩家用户名);
                    plr.SendInfoMessage($"已删除玩家：{玩家用户名}的称号");
                    return;
                default:
                    plr.SendInfoMessage($"设置指令:/称号 玩家名 称号位置 称号名");
                    plr.SendInfoMessage($"删除指令:/称号 玩家名 删除");
                    plr.SendInfoMessage($"称号位置:前前缀、前缀、角色名、后缀、后后缀、R、G、B");
                    return;
            }
            Add(玩家用户名, 前前缀, 前缀, 角色名, 后缀, 后后缀, R, G, B);
            plr.SendInfoMessage($"设置成功\n{玩家用户名}的称号为:\n前前缀：{前前缀}\n前缀：{前缀}\n角色名：{角色名}\n后缀：{后缀}\n后后缀：{后后缀}");
        }
        public static void Remove(string 玩家用户名)//删除称号
        {
            称号配置.称号设置.RemoveAll(s => (s.玩家用户名 == 玩家用户名));
            称号插件配置表.Write();
        }
        public static void Add(string 玩家用户名, string? 前前缀, string? 前缀, string? 角色名, string? 后缀, string? 后后缀, byte R, byte G, byte B)//添加称号
        {
            var z = 称号配置.称号设置.Find(s => s.玩家用户名 == 玩家用户名);
            if (z != null)
            {

                z.前前缀 = 前前缀;
                z.前缀 = 前缀;
                z.角色名 = 角色名;
                z.后缀 = 后缀;
                z.后后缀 = 后后缀;
                z.颜色.R = R;
                z.颜色.G = G;
                z.颜色.B = B;
            }
            else
            {
                称号配置.称号设置.Add(new 称号插件配置表.名称() { 玩家用户名 = 玩家用户名, 前前缀 = 前前缀, 前缀 = 前缀, 角色名 = 角色名, 后缀 = 后缀, 后后缀 = 后后缀, 颜色 = new() { R = R, G = G, B = B } });
            }
            称号插件配置表.Write();
        }
        #endregion
        public static string[] 获取聊天称号(TSPlayer plr)
        {
            if (!称号信息.ContainsKey(plr.Name))
            {
                称号信息.Add(plr.Name, new 称号插件配置表.聊天信息() { 前前缀 = "", 前缀 = null, 角色名 = null, 后缀 = null, 后后缀 = "" });
            }
            var z = 称号配置.称号设置.Find(s => s.玩家用户名 == plr.Name);//找到玩家称号
            if (z != null)//是否有称号，称号优先级最高
            {
                string 前前缀;
                string 前缀;
                string 角色名;
                string 后缀;
                string 后后缀;
                byte R = z.颜色.R;
                byte G = z.颜色.G;
                byte B = z.颜色.B;
                var c = 称号信息[plr.Name];
                if (z.前前缀 == null || z.前前缀 == "")//如果对于位置称号为空。则使用称号信息中的称号
                {
                    c.前前缀 ??= "";//如果称号信息的值为null，则设为"".
                    前前缀 = c.前前缀;
                }
                else
                {
                    前前缀 = z.前前缀;
                }
                前缀 = z.前缀 ?? c.前缀 ?? plr.Group.Prefix;
                角色名 = z.角色名 ?? c.角色名 ?? plr.Name;
                后缀 = z.后缀 ?? c.后缀 ?? plr.Group.Suffix;
                if (z.后后缀 == null || z.后后缀 == "")//如果对于位置称号为空。则使用称号信息中的称号
                {
                    c.后后缀 ??= "";
                    后后缀 = c.后后缀;
                }
                else
                {
                    后后缀 = z.后后缀;
                }
                if (R == 255) R = c.颜色.R;//如果颜色是默认的,则使用称号信息中的颜色
                if (G == 255) G = c.颜色.G;
                if (B == 255) B = c.颜色.B;
                string[] str = { 前前缀, 前缀, 角色名, 后缀, 后后缀, R.ToString(), G.ToString(), B.ToString() };
                c.前前缀 = ""; c.前缀 = null; c.角色名 = null; c.后缀 = null; c.后后缀 = "";
                return str;
            }
            else//如果没称号
            {
                var c = 称号信息[plr.Name];
                string 前前缀;
                string 前缀;
                string 角色名;
                string 后缀;
                string 后后缀;
                c.前前缀 ??= "";
                前前缀 = c.前前缀;
                前缀 = c.前缀 ?? plr.Group.Prefix;
                角色名 = c.角色名 ?? plr.Name;
                后缀 = c.后缀 ?? plr.Group.Suffix;
                c.后后缀 ??= "";
                后后缀 = c.后后缀;
                string[] str = { 前前缀, 前缀, 角色名, 后缀, 后后缀, c.颜色.R.ToString(), c.颜色.G.ToString(), c.颜色.B.ToString() };
                c.前前缀 = ""; c.前缀 = null; c.角色名 = null; c.后缀 = null; c.后后缀 = "";
                return str;
            }
        }

        private static void 聊天(ServerChatEventArgs args)
        {
            if (!配置.是否开启称号插件) return;
            if (!聊天检测(args)) return;
            var plr = TShock.Players[args.Who];
            var c = 获取聊天称号(plr);
            发送聊天(args, args.Text, c[0], c[1], c[2], c[3], c[4], byte.Parse(c[5]), byte.Parse(c[6]), byte.Parse(c[7]));
        }
        /// <summary>
        /// 发送聊天 返回值判断是否发送成功
        /// </summary>
        /// <param name="args"></param>
        /// <param name="聊天内容"></param>
        /// <param name="前前缀"></param>
        /// <param name="前缀"></param>
        /// <param name="角色名"></param>
        /// <param name="后缀"></param>
        /// <param name="后后缀"></param>
        /// <returns></returns>

        public static bool 发送聊天(ServerChatEventArgs args, string 聊天内容, string 前前缀, string 前缀, string 角色名, string 后缀, string 后后缀)
        {
            string 发送内容;
            if (args.Handled) return false;
            var plr = TShock.Players[args.Who];
            string 聊天格式 = 配置.聊天格式.Replace("{前前缀}", "{0}").Replace("{前缀}", "{1}").Replace("{角色名}", "{2}").Replace("{后缀}", "{3}").Replace("{后后缀}", "{4}").Replace("{聊天内容}", "{5}");
            发送内容 = String.Format(聊天格式, 前前缀, 前缀, 角色名, 后缀, 后后缀, 聊天内容);
            聊天气泡(args, 前前缀, 前缀, 角色名, 后缀, 后后缀);//先尝试发送聊天气泡
            if (args.Handled) return true;//如果true，说明已经发送了聊天气泡
            TSPlayer.All.SendMessage(发送内容, plr.Group.R, plr.Group.G, plr.Group.B);
            TSPlayer.Server.SendMessage(发送内容, plr.Group.R, plr.Group.G, plr.Group.B); //经过实践，[c/FFFFFF:],[i:]仍然生效
            TShock.Log.Info($"{plr.Name}>>> {发送内容}");
            args.Handled = true;
            return true;
        }
        public static bool 发送聊天(ServerChatEventArgs args, string 聊天内容, string 前前缀, string 前缀, string 角色名, string 后缀, string 后后缀, byte R, byte G, byte B)
        {
            string 发送内容;
            if (args.Handled) return false;
            string 聊天格式 = 配置.聊天格式.Replace("{前前缀}", "{0}").Replace("{前缀}", "{1}").Replace("{角色名}", "{2}").Replace("{后缀}", "{3}").Replace("{后后缀}", "{4}").Replace("{聊天内容}", "{5}");
            发送内容 = String.Format(聊天格式, 前前缀, 前缀, 角色名, 后缀, 后后缀, 聊天内容);
            聊天气泡(args, 前前缀, 前缀, 角色名, 后缀, 后后缀);//先尝试发送聊天气泡
            if (args.Handled) return true;//如果true，说明已经发送了聊天气泡
            TSPlayer.All.SendMessage(发送内容, R, G, B);
            TSPlayer.Server.SendMessage(发送内容, R, G, B); //经过实践，[c/FFFFFF:],[i:]仍然生效
            TShock.Log.Info($"{TShock.Players[args.Who].Name}>>> {发送内容}");
            args.Handled = true;
            return true;
        }
        public static void 聊天气泡(ServerChatEventArgs args, string 前前缀, string 前缀, string 角色名, string 后缀, string 后后缀)
        {
            if (!配置.是否开启聊天气泡) return;
            string 名称格式 = 配置.聊天气泡启用时玩家名称格式.Replace("{前前缀}", "{0}").Replace("{前缀}", "{1}").Replace("{角色名}", "{2}").Replace("{后缀}", "{3}").Replace("{后后缀}", "{4}");
            TSPlayer tsplr = TShock.Players[args.Who];
            string 玩家名称 = String.Format(名称格式, 前前缀, 前缀, 角色名, 后缀, 后后缀);
            string 玩家原名称 = tsplr.Name;
            Main.player[args.Who].name = 玩家名称;
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, Terraria.Localization.NetworkText.FromLiteral(玩家名称), args.Who, 0, 0, 0, 0);//发送数据包修改玩家名称
            Main.player[args.Who].name = 玩家原名称;
            Terraria.Net.NetPacket packet = Terraria.GameContent.NetModules.NetTextModule.SerializeServerMessage
            (
                Terraria.Localization.NetworkText.FromLiteral(args.Text), new Microsoft.Xna.Framework.Color(tsplr.Group.R, tsplr.Group.G, tsplr.Group.B), (byte)args.Who
            );
            Terraria.Net.NetManager.Instance.Broadcast(packet);
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, Terraria.Localization.NetworkText.FromLiteral(玩家原名称), args.Who, 0, 0, 0, 0);//发送数据包修改玩家名称
                                                                                                                                                 //玩家视野[c/FFFFFF:]不生效,[i:]生效
            TSPlayer.Server.SendMessage($"<{玩家名称}> " + args.Text, tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);//在控制台，[c/FFFFFF:],[i:]仍然生效
            TShock.Log.Info($"{玩家原名称}>>> <{玩家名称}> " + args.Text
                );//写入日志
            args.Handled = true;
        }
        public static void 聊天气泡(ServerChatEventArgs args, string 前前缀, string 前缀, string 角色名, string 后缀, string 后后缀, byte R, byte G, byte B)
        {
            if (!配置.是否开启聊天气泡) return;
            string 名称格式 = 配置.聊天气泡启用时玩家名称格式.Replace("{前前缀}", "{0}").Replace("{前缀}", "{1}").Replace("{角色名}", "{2}").Replace("{后缀}", "{3}").Replace("{后后缀}", "{4}");
            TSPlayer tsplr = TShock.Players[args.Who];
            string 玩家名称 = String.Format(名称格式, 前前缀, 前缀, 角色名, 后缀, 后后缀);
            string 玩家原名称 = tsplr.Name;
            Main.player[args.Who].name = 玩家名称;
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, Terraria.Localization.NetworkText.FromLiteral(玩家名称), args.Who, 0, 0, 0, 0);//发送数据包修改玩家名称
            Main.player[args.Who].name = 玩家原名称;
            Terraria.Net.NetPacket packet = Terraria.GameContent.NetModules.NetTextModule.SerializeServerMessage
            (
                Terraria.Localization.NetworkText.FromLiteral(args.Text), new Microsoft.Xna.Framework.Color(R, G, B), (byte)args.Who
            );
            Terraria.Net.NetManager.Instance.Broadcast(packet);
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, Terraria.Localization.NetworkText.FromLiteral(玩家原名称), args.Who, 0, 0, 0, 0);//发送数据包修改玩家名称
                                                                                                                                                 //玩家视野[c/FFFFFF:]不生效,[i:]生效
            TSPlayer.Server.SendMessage($"<{玩家名称}> " + args.Text, R, G, B);//在控制台，[c/FFFFFF:],[i:]仍然生效
            TShock.Log.Info($"{玩家原名称}>>> <{玩家名称}> " + args.Text
                );//写入日志
            args.Handled = true;
        }
        /// <summary>
        /// 判断玩家是否可以聊天
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool 聊天检测(ServerChatEventArgs args)
        {
            if (!是否指令(args.Text))//是否指令，“/”不是指令
            {
                var plr = TShock.Players[args.Who];
                if (plr.IsLoggedIn)//是否登录
                {
                    if (args.Handled)
                    {
                        return false;
                    }
                    if (args.Text == "")
                    {
                        args.Handled = true;
                        return false;
                    }
                    if (args.Text.Length > 500)
                    {
                        plr.SendErrorMessage("您发送的信息过长！");
                        args.Handled = true;
                        return false;
                    }
                    if (!plr.HasPermission(Permissions.canchat))
                    {
                        plr.SendErrorMessage("您没有聊天所需的权限\"" + Permissions.canchat + "\"");
                        args.Handled = true;
                        return false;
                    }
                    if (plr.mute)
                    {
                        plr.SendErrorMessage("您正被禁言中！");
                        args.Handled = true;
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public static bool 是否指令(string text)
        {
            if ((text.StartsWith(TShock.Config.Settings.CommandSpecifier) || text.StartsWith(TShock.Config.Settings.CommandSilentSpecifier))
                && !string.IsNullOrWhiteSpace(text[1..])) return true;
            return false;
        }
    }
}