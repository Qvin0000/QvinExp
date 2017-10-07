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
            SortLikeHuman = false;
            NewSorting = true;
        }


        [Menu("Extra Delay", 2000)]
        public RangeNode<int> ExtraDelay { get; set; }
        [Menu("Sort Like Human",  3000)]
        public ToggleNode SortLikeHuman { get; set; }
        [Menu("New sorting", 4000)]
        public ToggleNode NewSorting { get; set; }
    }
}