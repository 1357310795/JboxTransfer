using JboxTransfer.Core.Helpers;
using JboxTransfer.Core.Modules;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using System.Net;
using Teru.Code.Models;

namespace JboxTransfer.Core.Services
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
    public class GlobalSettings : StorageableBase
    {
        public static GlobalSettings Default = new GlobalSettings();
        public override string FileName => "settings.json";
        public override StorageMode Mode => StorageMode.AppdataFolder;
        public SettingsModel Model { get; set; }

        public void Save()
        {
            Save(JsonConvert.SerializeObject(Model));
        }

        public SettingsModel Read()
        {
            Model = new SettingsModel() { WorkThreads = 4 };
            try
            {
                var json = base.Read();
                Model = JsonConvert.DeserializeObject<SettingsModel>(json);
            }
            catch (Exception ex)
            {

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
