using JboxTransfer.Core.Helpers;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Net;
using Teru.Code.Models;

namespace JboxTransfer.Services
{
    public class SettingsModel
    {
        /// <summary>
        /// 0:浅色；1:深色
        /// </summary>
        public int ThemeMode { get; set; }
        public string ThemeColor { get; set; }
        public int WorkThreads { get; set; }
    }
    public static class GlobalSettings
    {
        public static SettingsModel Model { get; set; }
        static GlobalSettings()
        {
            _fileName = Path.Combine(PathHelper.AppDataPath, "settings.json");
        }

        private static string _fileName;

        public static void Save()
        {
            Directory.CreateDirectory(PathHelper.AppDataPath);
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(Model));
        }

        public static SettingsModel Read()
        {
            Model = new SettingsModel();
            if (File.Exists(_fileName))
            {
                var json = File.ReadAllText(_fileName);
                Model = JsonConvert.DeserializeObject<SettingsModel>(json);
            }
            return Model;
        }

        //public static CommonResult Clear()
        //{
        //    try
        //    {
        //        File.Delete(_fileName);
        //        CookieContainer = new CookieContainer();
        //        return new CommonResult(true, "");
        //    }
        //    catch(Exception ex)
        //    {
        //        return new CommonResult(false, ex.Message);
        //    }
        //}

    }
}
