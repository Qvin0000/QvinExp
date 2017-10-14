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
            DebugPerformance = false;
            PickUpEnable = true;
            ChestClickEnable = true;
            NoItemsClickChest = true;
        }

        [Menu("Pick up enable")]
        public ToggleNode PickUpEnable { get; set; }
        [Menu("No items for pick - click chest")]
        public ToggleNode NoItemsClickChest { get; set; }
        [Menu("Chest click enable")]
        public ToggleNode ChestClickEnable { get; set; }
        [Menu("Extra Delay")]
        public RangeNode<int> ExtraDelay { get; set; }
        [Menu("Sort Like Human")]
        public ToggleNode SortLikeHuman { get; set; }
        [Menu("New sorting")]
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

        [Menu("Debug Performance")]
        public ToggleNode DebugPerformance { get; set; }
    }
}