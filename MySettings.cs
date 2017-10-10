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
            PickupPriorityRange = new RangeNode<int>(500, 1, 1000);
            PickupRange = new RangeNode<int>(750, 1, 1000);
            PickupTimerDelay = new RangeNode<int>(100, 1, 1000);
            ChestRange = new RangeNode<int>(500, 1, 1000);
            CacheTimer = new RangeNode<int>(200, 1, 1000);
            fps = new RangeNode<int>(0, 0, 60);
        }

     
        [Menu("Extra Delay", 2000)]
        public RangeNode<int> ExtraDelay { get; set; }
        [Menu("Sort Like Human",  3000)]
        public ToggleNode SortLikeHuman { get; set; }
        [Menu("New sorting", 4000)]
        public ToggleNode NewSorting { get; set; }

        [Menu("Pickup Priority Radius")]
        public RangeNode<int> PickupPriorityRange { get; set; }
        [Menu("Pickup Radius")]
        public RangeNode<int> PickupRange { get; set; }
        [Menu("Click chest Radius")]
        public RangeNode<int> ChestRange { get; set; }
        [Menu("Pickup Timer Delay")]
        public RangeNode<int> PickupTimerDelay { get; set; }
        [Menu("Cache timer (experiment)")]
        public RangeNode<int> CacheTimer { get; set; }
        [Menu("PoeHUD FPS limit")]
        public RangeNode<int> fps { get; set; }
    }
}