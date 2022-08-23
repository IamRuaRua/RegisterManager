using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TerrariaApi.Server;
using TShockAPI;

namespace RegisterManager
{
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin
    {
        Config configFile;
        public override string Name => "RegisterManager";

        public override string Description => "规范玩家用户名,限制user命令权限";

        public override string Author => "rua";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public Main(Terraria.Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);//玩家加入
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);//服务器启动后执行
            Commands.ChatCommands.Single((Command c) => c.Permissions.Contains(Permissions.canregister)).CommandDelegate = new CommandDelegate(register);//重写注册功能
            Commands.ChatCommands.Add(new Command(permissions: "registermanager", cmd: this.usr, "usr"));
            Config.EnsureFile();
            configFile = Config.ReadConfig();
        }
        private void OnPostInitialize(EventArgs e)
        {
            if (configFile.移除guest组注册权限)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("已移除guest组注册权限");
                RunCMDOnTshock("group delperm guest tshock.account.register");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("已给予guest组注册权限");
                RunCMDOnTshock("group addperm guest tshock.account.register");
                Console.ResetColor();
            }
        }

        void OnServerJoin(JoinEventArgs args)
        {
            var player = TShock.Players[args.Who];
            configFile = Config.ReadConfig();
            if (configFile.用户名不规范无法加入)
            {
                if (!CheckPlayerNameGetBool(player.Name))
                    RunCMDOnTshock("kick " + player.Name + " 用户名不规范,"+ CheckPlayerNameGetString(player.Name) + ",请新建角色加入.");
            }
        }

        private void register(CommandArgs args)
        {
            TSPlayer player = args.Player;
            if (TShock.UserAccounts.GetUserAccountByName(player.Name) != null)
            {
                player.SendErrorMessage("账号已被注册，请更换角色进入");
                return;
            }
            if (args.Parameters.Count > 0)
            {
                configFile = Config.ReadConfig();
                if (configFile.用户名不规范无法加入)
                {
                    if (!CheckPlayerNameGetBool(player.Name))
                        RunCMDOnTshock("kick " + player.Name + " 用户名不规范," + CheckPlayerNameGetString(player.Name) + ",请新建角色加入.");
                }
               var newPlayerPassword=args.Parameters[0];
                char[] PasswordArr = newPlayerPassword.ToCharArray();
                for (int i = 0; i < PasswordArr.Length; i++)
                {
                    if (!Char.IsLetterOrDigit(PasswordArr[i]))
                    {
                        player.SendErrorMessage("密码不能包含特殊字符");
                        return;
                    }
                }
                //添加玩家
                var cmd = "/user add " + player.Name + " " + newPlayerPassword+" default";
                player.SendInfoMessage("注册成功,请使用/login 密码 来登录游戏");
                RunCMDOnTshock(cmd);
            }
            else
            {
                player.SendInfoMessage("请使用/register 密码注册");
            }
          
        }
        //usr命令
        private void usr(CommandArgs args)
        {
            configFile = Config.ReadConfig();
            var cmdArgs = args.Parameters;
            var sendPlayer = args.Player;
            try
            {
                switch (cmdArgs[0])
                {
                    case "help":
                        sendPlayer.SendMessage("/ usr add username password          -- 添加新用户"
                                           + "\n/ usr del username                   -- 删除一个用户"
                                           + "\n/ usr password username newpassword  -- 修改一个用户密码"
                                           + "\n/ usr joinCheck                      -- 开启或关闭加入游戏时的用户名规范验证"
                                           + "\n/ usr regperm                         -- 给予或收回guest用户组的注册权限", new Microsoft.Xna.Framework.Color(255,255,0));
                        return;
                    case "add":
                    case "Add":
                        RegisterNewUser(sendPlayer, cmdArgs[1], cmdArgs[2]);
                        return;
                    case "del":
                    case "Del":
                        DeleteUser(cmdArgs[1]);
                        return;
                    case "password":
                    case "Password":
                        ChangeUserPassword(sendPlayer, cmdArgs[1], cmdArgs[2]); 
                        return;
                    case "joincheck":
                    case "JoinCheck":
                    case "joinCheck":
                        if (configFile.用户名不规范无法加入)
                        {
                            sendPlayer.SendInfoMessage("已关闭加入游戏时的用户名规范验证,再次使用开启");
                        }
                        else
                        {
                            sendPlayer.SendInfoMessage("已开启加入游戏时的用户名规范验证,再次使用关闭");
                        }
                        Config.WriteConfig(!configFile.用户名不规范无法加入, configFile.移除guest组注册权限);
                        return;
                    case "regperm":
                    case "Regperm":
                        if (!configFile.移除guest组注册权限)
                        {
                            sendPlayer.SendInfoMessage("已移除guest用户组的注册权限,再次使用给予权限");
                            RunCMDOnTshock("group delperm guest tshock.account.register");
                        }
                        else
                        {
                            sendPlayer.SendInfoMessage("已给予guest用户组的注册权限,再次使用移除权限");
                            RunCMDOnTshock("group addperm guest tshock.account.register");
                        }
                        Config.WriteConfig(configFile.用户名不规范无法加入, !configFile.移除guest组注册权限);
                        return;
                    default:
                        sendPlayer.SendErrorMessage("语法错误,使用/usr help获取帮助");
                        return;
                }
            }
            catch (Exception e)
            {
                sendPlayer.SendErrorMessage("语法错误,使用/usr help获取帮助");
                return;
            }
        }
        void DeleteUser(string PlayerName)
        {
            var cmd = "/user del " + PlayerName; 
             RunCMDOnTshock(cmd);

        }
        void ChangeUserPassword(TSPlayer sendPlayer, string PlayerName, string newPlayerPassword)
        {
            char[] PasswordArr = newPlayerPassword.ToCharArray();
            for (int i = 0; i < PasswordArr.Length; i++)
            {
                if (!Char.IsLetterOrDigit(PasswordArr[i]))
                {
                    sendPlayer.SendErrorMessage("密码不能包含特殊字符");
                    return;
                }
            }
            //修改玩家密码
            var cmd = "/user password " + PlayerName + " " + newPlayerPassword; 
             RunCMDOnTshock(cmd);

        }
        void RegisterNewUser(TSPlayer sendPlayer,string newPlayerName,string newPlayerPassword) {
            //检测用户名是否规范
            if (!CheckPlayerNameGetBool(newPlayerName))
            {
                sendPlayer.SendErrorMessage(CheckPlayerNameGetString(newPlayerName));
                return;
            }
                //检测密码是否规范
                char[] PasswordArr = newPlayerPassword.ToCharArray();
            for (int i = 0; i < PasswordArr.Length; i++)
            {
                if (!Char.IsLetterOrDigit(PasswordArr[i]))
                {
                    sendPlayer.SendErrorMessage("密码不能包含特殊字符");
                    return;
                }
            }
            //注册新玩家
            var cmd = "/user add " + newPlayerName + " " + newPlayerPassword+ " default";
             RunCMDOnTshock(cmd);

        }

        bool CheckPlayerNameGetBool(string PlayerName)
        {
            //检测昵称是否规范
            bool checkResult= true;
            string reason = "";
            if (new Regex("^[0-9].+").IsMatch(PlayerName))
            {
                checkResult=false;
                reason = "用户名不能以数字开头";
            }
            char[] NameArr = PlayerName.ToCharArray();
            if (NameArr.Length > 10)
            {
                checkResult = false;
                reason = "昵称过长";
            }
            for (int i = 0; i < NameArr.Length; i++)
            {
                if (!Char.IsLetterOrDigit(NameArr[i]))
                {
                    checkResult = false;
                    reason = "昵称包含特殊字符";
                }
            }
            return checkResult;
        }
        string CheckPlayerNameGetString(string PlayerName)
        {
            //检测昵称是否规范
            bool checkResult = true;
            string reason = "";
            if (new Regex("^[0-9].+").IsMatch(PlayerName))
            {
                checkResult = false;
                reason = "用户名不能以数字开头";
            }
            char[] NameArr = PlayerName.ToCharArray();
            if (NameArr.Length > 10)
            {
                checkResult = false;
                reason = "昵称过长";
            }
            for (int i = 0; i < NameArr.Length; i++)
            {
                if (!Char.IsLetterOrDigit(NameArr[i]))
                {
                    checkResult = false;
                    reason = "昵称包含特殊字符";
                }
            }
            return reason;
        }

        void RunCMDOnTshock(string command)
        {

            Console.WriteLine("usr命令:"+command);
            CommandEventArgs args = new CommandEventArgs();
            var prop = args.GetType().GetProperty("Command");
            prop.SetValue(args, command);
            ServerApi.Hooks.ServerCommand.Invoke(args);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
