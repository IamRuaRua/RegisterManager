using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace RegisterManager
{
    public class Config
    {
        //路径
        public static string ConfigPath = $"{TShock.SavePath}/RegisterManager.json";
        public bool 用户名不规范无法加入;//默认开启
        public bool 移除guest组注册权限;//默认关闭
        /// <summary>
        /// 确认配置文件存在，不存在则创建并填入默认值
        /// </summary>
        public static void EnsureFile()
        {
            if (!File.Exists(ConfigPath))
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config(true,false)));
            }
        }
        public static Config ReadConfig()
        {
            //读取ConfigFile
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
        }
        public Config(bool bool1,bool bool2)
        {
            用户名不规范无法加入= bool1;
            移除guest组注册权限 = bool2;
        }
        public static void WriteConfig(bool bool1, bool bool2)
        {
             File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config(bool1, bool2)));
        }
    }
}
