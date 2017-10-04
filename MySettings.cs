using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace QvinExp
{
    public class MySettings : SettingsBase
    {

        public MySettings()
        {

            Enable = true;
            ExtraDelay = new RangeNode<int>(0, 0, 2000);
        }


        [Menu("Extra Delay", 2000)]
        public RangeNode<int> ExtraDelay { get; set; }
    }
}