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
        public override string Description => "称号插件";
        /// 插件名字
        public override string Name => "称号插件";
        /// 插件版本
        public override Version Version => new Version(1, 0, 0, 0);
        /// 插件处理
        public 称号插件(Main game) : base(game)
        {
        }
        /// 插件启动时，用于初始化各种狗子
        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);//钩住游戏初始化时
            ServerApi.Hooks.ServerChat.Register(this, _聊天);
            称号插件配置表.GetConfig();
        }
        /// 插件关闭时
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);//销毁游戏初始化狗子
                ServerApi.Hooks.ServerChat.Deregister(this, _聊天);

            }
            base.Dispose(disposing);
        }

        private void OnInitialize(EventArgs args)//游戏初始化的狗子
        {
            //第一个是权限，第二个是子程序，第三个是名字
        }
        private void _聊天(ServerChatEventArgs _聊天内容)
        {
            try
            {
                if (称号插件配置表.GetConfig().是否开启称号插件 == true && TShock.Players[_聊天内容.Who].Group.Name != "guest" && _聊天内容.Text.Substring(0, 1) != "/" && _聊天内容.Text.Substring(0, 1) != ".")
                {
                    foreach (var z in 称号插件配置表.GetConfig().称号设置)
                    {
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
                            TSPlayer.Server.SendMessage(text, 255, 255, 255);
                            _聊天内容.Handled = true;
                        }
                    }
                }
            }
            catch
            {
                TSPlayer.Server.SendErrorMessage($"[称号插件]配置文件读取错误！");
            }
        }
    }
}