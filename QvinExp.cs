using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using PoeHUD.DebugPlug;
using PoeHUD.Framework.Helpers;
using PoeHUD.Models;
using PoeHUD.Models.Enums;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.Elements;
using PoeHUD.Poe.RemoteMemoryObjects;
using QvinExp.Utils;
using SharpDX;
using Map = PoeHUD.Poe.Components.Map;

namespace QvinExp
{
    public class QvinExp : BaseSettingsPlugin<MySettings>
    {
        private Vector2 _clickWindowOffset;
        private readonly bool _Debug = false;
        private int[,] _ignoredCells;
        private bool _Working;
        private readonly List<EntityWrapper> entities = new List<EntityWrapper>();
        private readonly int INPUT_DELAY = 15;
        private readonly int UPDATE_DELAY = 256;
        private readonly int PIXEL_BORDER = 3;
        private readonly Stopwatch pickUpTimer = Stopwatch.StartNew();
        private readonly Stopwatch cacheTimer = Stopwatch.StartNew();
        public override void Initialise()
        {
            LoadIgnoredCells();
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
        }
        private void DebugPerformance()
        {
            DebugPlugin.LogInfoMsg($"FPS Infinite While: {Graphics.FpsLoop}");
            DebugPlugin.LogInfoMsg($"FPS Data: {Graphics.FpsData}");
            DebugPlugin.LogInfoMsg($"FPS Render: {Graphics.FpsRender}");
            DebugPlugin.LogInfoMsg($"Thread Sleep: {Graphics.Sleep}");

        }
        public override void Render()
        {

            if(Settings.DebugPerformance)
                DebugPerformance();

            if (Keyboard.IsKeyDown((int) Keys.F1) && Settings.PickUpEnable)
            {
                if (_Working)
                    return;
                _Working = true;
                NewPickUp();
            }

            if (Keyboard.IsKeyDown((int)Keys.F2) && Settings.ChestClickEnable)
            {

                if (_Working)
                    return;
                _Working = true;
                 ClickOnChests();
            }

            if (Keyboard.IsKeyDown((int) Keys.PageDown) && GameController.Game.IngameState.IngameUi.InventoryPanel
                    .IsVisible)
            {
                if (_Working)
                    return;
                _Working = true;
                IdentifInventory();
                return;
            }
            if (Keyboard.IsKeyDown((int) Keys.PageUp) && GameController.Game.IngameState.IngameUi.InventoryPanel
                    .IsVisible)
            {
                if (_Working)
                    return;
                _Working = true;
                SellItemsFromInventory();
                return;
            }

            if (Keyboard.IsKeyDown((int) Keys.Pause) && GameController.Game.IngameState.IngameUi.InventoryPanel
                    .IsVisible)
            {
                if (_Working)
                    return;
                _Working = true;
                SellUnidenItemsFromInventory();
                return;
            }
            var uiTabsOpened = GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible &&
                               GameController.Game.IngameState.ServerData.StashPanel.IsVisible;

            if (!uiTabsOpened)
            {
                _Working = false;
                return;
            }

            if (Keyboard.IsKeyDown((int) Keys.End))
            {
                if (_Working)
                    return;
                _Working = true;
                if(Settings.NewSorting)
                SortItems();
                else
                SortMapsMain();
                return;
            }
            if (Keyboard.IsKeyDown((int) Keys.Home))
            {
                if (_Working)
                    return;
                _Working = true;
                Get3Maps();
            }
        }

        public override void EntityAdded(EntityWrapper entityWrapper)
        {
            entities.Add(entityWrapper);
        }

        public override void EntityRemoved(EntityWrapper entityWrapper)
        {
            entities.Remove(entityWrapper);
        }




        #region Old Function

        private void SellUnidenItemsFromInventory()
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var inv = GameController.Game.IngameState.IngameUi.InventoryPanel[
                InventoryIndex.PlayerInventory];
            var items = inv.VisibleInventoryItems;
            var itemsData = GetItemsDataFromInventoryItems(items);
            var listItems = itemsData.Where(x => !x.BIdentified && x.Rarity == ItemRarity.Rare).ToList();
            Keyboard.KeyDown(Keys.LControlKey);
            foreach (var item in listItems)
            {
                if (CheckIgnoreCells(item._inventoryItem)) continue;
                var unPos = item.GetClickPos() + _clickWindowOffset;
                Mouse.SetCursorPosAndLeftClick(unPos, Settings.ExtraDelay);
                Mouse.SetCursorPosAndLeftClick(unPos, INPUT_DELAY);
                Mouse.SetCursorPosAndLeftClick(unPos, INPUT_DELAY);
                _Working = false;
            }
            Keyboard.KeyUp(Keys.LControlKey);
            _Working = false;
        }

        private void SellItemsFromInventory()
        {
            var inv = GameController.Game.IngameState.IngameUi.InventoryPanel[
                InventoryIndex.PlayerInventory];
            var items = inv.VisibleInventoryItems;
            var itemsData = GetItemsDataFromInventoryItems(items);
            var listItems = itemsData.Where(x => x.BIdentified &&
                                                 (x.Rarity == ItemRarity.Rare || x.Rarity == ItemRarity.Unique ||
                                                  x.Rarity == ItemRarity.Magic))
                .ToList();
            Keyboard.KeyDown(Keys.LControlKey);
            foreach (var item in listItems)
            {
                if (CheckIgnoreCells(item._inventoryItem)) continue;
                var unPos = item.GetClickPos() + _clickWindowOffset;
                Mouse.SetCursorPosAndLeftClick(unPos, Settings.ExtraDelay);
                Mouse.SetCursorPosAndLeftClick(unPos, INPUT_DELAY);
                Mouse.SetCursorPosAndLeftClick(unPos, INPUT_DELAY);
                _Working = false;
            }
            Keyboard.KeyUp(Keys.LControlKey);
            _Working = false;
        }

        private void IdentifInventory()
        {
            var inv = GameController.Game.IngameState.IngameUi.InventoryPanel[
                InventoryIndex.PlayerInventory];
            var items = inv.VisibleInventoryItems;
            var itemsData = GetItemsDataFromInventoryItems(items);
            var scroll = itemsData.Where(x => x.BaseName.ToLower().Contains("scroll of w".ToLower())).ToList();
            var uniden = itemsData.Where(x => x.BIdentified == false &&
                                              (x.Rarity == ItemRarity.Rare || x.Rarity == ItemRarity.Unique))
                .ToList();

            if (scroll.Count > 0 && uniden.Count > 0)
            {
                _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                var scrollPosition = scroll.First().GetClickPos() + _clickWindowOffset;
                Keyboard.KeyDown(Keys.LShiftKey);
                Thread.Sleep(INPUT_DELAY);
                Mouse.SetCursorPos(scrollPosition);
                Thread.Sleep(INPUT_DELAY + Settings.ExtraDelay);
                Mouse.RightMouseDown();
                Thread.Sleep(INPUT_DELAY);
                Mouse.RightMouseUp();
                foreach (var itemData in uniden)
                {
                    var unPos = itemData.GetClickPos() + _clickWindowOffset;
                    Mouse.SetCursorPosAndLeftClick(unPos, Settings.ExtraDelay);
                    _Working = false;
                }
                Thread.Sleep(INPUT_DELAY);
                Keyboard.KeyUp(Keys.LShiftKey);
            }
            _Working = false;
        }

        private void Get3Maps(int howMany = 48)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var stashItems = stash.VisibleInventoryItems.ToList();
            var sortedMaps = SortMaps(stashItems);
            var grouped = sortedMaps.GroupBy(x => x.ItemData.BaseName);
            var moreThan3Map = new List<ItemQ>();
            foreach (var gr in grouped)
            {
                if (gr.Count() < 3) continue;
                var mapsCount = gr.Count();
                var sellCount = (int)Math.Floor(mapsCount / 3.0) * 3;
                var tempList = gr.ToList();
                for (var i = 0; i < sellCount; i++)
                    moreThan3Map.Add(tempList[i]);
            }
            if (moreThan3Map.Count < 48)
                howMany = moreThan3Map.Count;
            for (var i = 0; i < howMany; i++)
            {
                var cellWOff = moreThan3Map[i].ItemData.GetClickPos() + _clickWindowOffset;
                Keyboard.KeyDown(Keys.LControlKey);
                Thread.Sleep(INPUT_DELAY);
                Mouse.SetCursorPosAndLeftClick(cellWOff, Settings.ExtraDelay);
                Thread.Sleep(INPUT_DELAY);
                Keyboard.KeyUp(Keys.LControlKey);
            }
            _Working = false;
        }
        #endregion

        #region Sort Function

        private delegate List<ItemQ> SortMethod(List<NormalInventoryItem> items);
        private List<ItemQ> MainSort()
        {
            SortMethod _sortMethod;
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var stashItems = stash.VisibleInventoryItems;
            _sortMethod = ChoiceSort(stashItems);
            var items = _sortMethod(stashItems);


        //var SizeInv = (int)stash.SizeInv;
            var SizeInv = FIXGetSizeInventory();
            var z = 0;
            for (int i = 0; i < SizeInv; i++)
            {
                for (int j = 0; j < SizeInv; j++)
                {
                    if (z < items.Count)
                    {
                        items[z].nX = i;
                        items[z].nY = j;
                        items[z].i = z;
                        z++;
                    }

                }
            }
            return items;
        }

        private SortMethod ChoiceSort(List<NormalInventoryItem> items)
        {

            var result = new List<Tuple<int, SortMethod>>();
            var ch = new List<Tuple<string[], SortMethod>>();
            ch.Add(new Tuple<string[], SortMethod>(new []{ "maps", "Metadata/Items/Labyrinth/OfferingToTheGoddess" },SortMaps));
            ch.Add(new Tuple<string[], SortMethod>(new []{ "gems"},SortGems));
            ch.Add(new Tuple<string[], SortMethod>(new [] { "amulets", "rings", "amulet" },SortJewelery));
            ch.Add(new Tuple<string[], SortMethod>(new []{ "jewels", },SortJewels));

            foreach (var tuple in ch)
            {
                var sum = 0;
                foreach (var s in tuple.Item1)
                {
                    sum += items.Count(x => x.Item.Path.ToLower().Contains(s.ToLower()));
                }
                result.Add(new Tuple<int, SortMethod>(sum,tuple.Item2));
            }
            var res = result.OrderByDescending(x => x.Item1).First();
            return res.Item2;

        }

        private List<ItemQ> RandomSort(List<NormalInventoryItem> items)
        {
            Random rnd = new Random();
            var itemsData = GetItemsDataFromInventoryItems(items);
            var itemsQ = (from itemData in itemsData
                let tier = rnd.Next(1, 100)
                select new ItemQ(tier, itemData)).ToList();
            var result = itemsQ.OrderBy(x => x.Tier)
                .ToList();
            return result;


        }
        private List<ItemQ> SortMaps(List<NormalInventoryItem> items)
        {
            var tempItems = items.Where(x => x.Item.Path.ToLower().Contains("maps") ||
                                              x.Item.Path.Contains("Metadata/Items/Labyrinth/OfferingToTheGoddess"))
                .ToList();
            var itemsData = GetItemsDataFromInventoryItems(tempItems);
            var itemsQ = (from iD in itemsData
                let tier = iD._inventoryItem.Item.GetComponent<Map>().Tier
                select new ItemQ(tier, iD)).ToList();

            var result = itemsQ.OrderBy(x => x.Tier)
                .ThenBy(y => y.ItemData.BaseName)
                .ThenBy(u => u.ItemData.Rarity)
                .ToList();
            return result;
        }

        private List<ItemQ> SortGems(List<NormalInventoryItem> items)
        {

            var tempItems = items.Where(x => x.Item.Path.ToLower().Contains("Gems".ToLower()))
                .ToList();

            var itemsData = GetItemsDataFromInventoryItems(tempItems);

            var itemsQ = (from iD in itemsData select new ItemQ(iD));
            var result = itemsQ.OrderBy(x => x.ItemData.ItemQuality).ThenBy(x=>x.ItemData.BaseName).ToList();
            return result;
        }


        private List<ItemQ> SortJewelery(List<NormalInventoryItem> items)
        {

            var tempItems = items.Where(x => x.Item.Path.ToLower().Contains("rings") ||
                                             x.Item.Path.ToLower().Contains("amulet") ||
                                             x.Item.Path.ToLower().Contains("amulets")).ToList();
            var itemsData = GetItemsDataFromInventoryItems(tempItems);
            var itemsQ = from iD in itemsData select new ItemQ(iD);
            var result = itemsQ.OrderBy(x => x.ItemData.ClassName)
                .ThenBy(x => x.ItemData.BaseName)
                .ThenBy(x=>x.ItemData.Path)
                .ThenBy(x => x.ItemData.Rarity)
                .ThenBy(x => x.ItemData.ItemLevel)
                .ThenBy(x=>x.ItemData.BIdentified).ToList();
            return result;
        }

        private List<ItemQ> SortJewels(List<NormalInventoryItem> items)
        {

            var tempItems = items.Where(x => x.Item.Path.ToLower().Contains("jewels")
                                           ).ToList();
            var itemsData = GetItemsDataFromInventoryItems(tempItems);
            var itemsQ = from iD in itemsData select new ItemQ(iD);
            var rarityJewels = itemsQ.Where(x => x.ItemData.Rarity != ItemRarity.Unique).OrderBy(x => x.ItemData.Rarity).ThenBy(x => x.ItemData.BaseName).ToList();
            var uniqueJewels = itemsQ.Where(x => x.ItemData.Rarity == ItemRarity.Unique).OrderBy(x => x.ItemData.BaseName).ThenBy(x => x.ItemData.ClassName).ToList();
            rarityJewels.AddRange(uniqueJewels);
            return rarityJewels;
        }
        #endregion



        private bool CheckIgnoreCells(NormalInventoryItem inventItem)
        {
            var inventPosX = inventItem.InventPosX;
            var inventPosY = inventItem.InventPosY;


            if (inventPosX < 0 || inventPosX >= 12)
                return true;
            if (inventPosY < 0 || inventPosY >= 5)
                return true;

            return _ignoredCells[inventPosY, inventPosX] != 0; //No need to check all item size
        }


        private void LoadIgnoredCells()
        {
            const string fileName = @"/IgnoredCells.json";
            var filePath = PluginDirectory + fileName;

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                try
                {
                    _ignoredCells = JsonConvert.DeserializeObject<int[,]>(json);


                    var ignoredHeight = _ignoredCells.GetLength(0);
                    var ignoredWidth = _ignoredCells.GetLength(1);

                    if (ignoredHeight != 5 || ignoredWidth != 12)
                        LogError("Stashie: Wrong IgnoredCells size! Should be 12x5. Reseting to default..", 5);
                    else
                        return;
                }
                catch (Exception ex)
                {
                    LogError(
                        "Stashie: Can't decode IgnoredCells settings in " + fileName +
                        ". Reseting to default. Error: " + ex.Message, 5);
                }
            }


            _ignoredCells = new[,]
            {
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}
            };

            var defaultSettings = JsonConvert.SerializeObject(_ignoredCells);
            defaultSettings = defaultSettings.Replace("[[", "[\n[");
            defaultSettings = defaultSettings.Replace("],[", "],\n[");
            defaultSettings = defaultSettings.Replace("]]", "]\n]");
            File.WriteAllText(filePath, defaultSettings);
        }


        #region Debug

        private void Debug(string message, int time = 1)
        {
            if (_Debug)
            {
                DebugPlugin.LogMsg(message, time);
                    File.AppendAllText("Debug.txt", message + Environment.NewLine);
            }
        }

        private void DebugObject(object obj)
        {
            if (!_Debug) return;
            var maxDepth = 3;
            var split = 50;
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var depth = 0;
            var debugOutput = new StringBuilder();
            void Recur(object o, ref StringBuilder str)
            {
                var oProp = o.GetType().GetProperties(flags).Where(x => x.GetIndexParameters().Length == 0);
                var notPrimitive = new List<Tuple<string, object>>();
                var space = new string(' ', 4 * depth);
                str.AppendLine(space + "Class Name: " +o.GetType().Name);
                foreach (var propertyInfo in oProp)
                    if (propertyInfo.GetValue(o, null).GetType().IsPrimitive ||
                        propertyInfo.GetValue(o, null) is decimal ||
                        propertyInfo.GetValue(o, null) is string ||
                        propertyInfo.GetValue(o, null) is TimeSpan
                    )
                        str.AppendLine(space + propertyInfo.Name + ": " + propertyInfo.GetValue(o, null));
                    else
                    {
                        notPrimitive.Add(
                            new Tuple<string, object>(propertyInfo.ToString(), propertyInfo.GetValue(o, null)));
                    }
                foreach (var o1 in notPrimitive)
                {
                    depth++;
                    if (depth >= maxDepth) return;
                    space = new string(' ', 4 * depth+2);
                    str.AppendLine(space + o1.Item1 + ": " + o1.Item2);
                    Recur(o1.Item2, ref str);
                }
            }
                debugOutput.AppendLine(new string('-', split));
                Recur(obj, ref debugOutput);
                debugOutput.AppendLine(new string('-', split));
                File.AppendAllText("DebugOject.txt", debugOutput.ToString());


        }

        private void DebugObject(IEnumerable<object> listObjects)
        {
            if (_Debug)
            {
                foreach (var o in listObjects)
                {
                   DebugObject(o);

                }
            }
        }
        #endregion

        #region SortMethodV3





        //Deprecated
        public List<NormalInventoryItem> SortMapsByInvPosition(IEnumerable<NormalInventoryItem> items)
        {
            var result = items.OrderBy(x => x.InventPosX).ThenBy(x => x.InventPosY).ToList();
            return result;
        }

        //Deprecated
        public void SortMapsMain()
        {
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var inventory = GameController.Game.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory];
            var invItems = inventory.VisibleInventoryItems;
            var sizeSorting = 58 - invItems.Count;
            var sizeSortingHalf = sizeSorting / 2;
            var iMax = stash.VisibleInventoryItems.Count / sizeSortingHalf + 1;
            for (var i = 0; i < iMax; i++)
            {
                var cacheItems = stash.VisibleInventoryItems.ToList();
                var sortedMaps = SortMaps(cacheItems);
                var finalSorted = sortedMaps.Skip(i * sizeSortingHalf).Take(sizeSortingHalf).ToList();
                ClickOnItemsWShift(finalSorted);
                var sortedMapsByInvPosition = SortMapsByInvPosition(cacheItems)
                    .Skip(i * sizeSortingHalf)
                    .Take(sizeSortingHalf)
                    .ToList();
                Thread.Sleep((int) (UPDATE_DELAY * 1.5));
                var sortedMapsByInvPosition2 = SortMapsByInvPosition(stash.VisibleInventoryItems.ToList()).ToList();
                var Interct = sortedMapsByInvPosition2.Intersect(sortedMapsByInvPosition);
                ClickOnItemsWShift(Interct);
                Thread.Sleep(UPDATE_DELAY);
                var sortedMapsFromInventory = SortMaps(inventory.VisibleInventoryItems.ToList());
                ClickOnItemsWShift(sortedMapsFromInventory);
                Thread.Sleep(UPDATE_DELAY);
            }
            _Working = false;
        }

        #endregion

        #region Pick up items and click on chest

        //Deprecated
        private void TestPickUp()
        {
            if (pickUpTimer.ElapsedMilliseconds < 100)
            {
                _Working = false;
                return;
            }
            pickUpTimer.Restart();
            var sortedByDistDropItems = new List<Tuple<int, long, EntityWrapper>>();


            foreach (var entity in entities)
            {
                Entity item = null;
                if (entity.Path.ToLower().Contains("worlditem"))
                    item = entity.GetComponent<WorldItem>().ItemEntity;

                if (item == null) continue;

                var en = item.Path.ToLower();
                var skip = false || en.Contains("currencyidentification") || en.Contains("CurrencyRerollMagicShard".ToLower()) || en.Contains("CurrencyUpgradeToMagicShard".ToLower());
                if (skip) continue;
                var d = GetEntityDistance(entity);
                var t = new Tuple<int, long, EntityWrapper>(d, entity.Address, entity);
                sortedByDistDropItems.Add(t);
            }


            var orderedByDistance = sortedByDistDropItems.OrderBy(x => x.Item1).ToList();

            var orderedButOnlyCurrencyAndMap = orderedByDistance.Where(
                x => (x.Item3.GetComponent<WorldItem>().ItemEntity.Path.ToLower().Contains("currency") ||
                      x.Item3.GetComponent<WorldItem>().ItemEntity.Path.ToLower().Contains("map") || x.Item3
                          .GetComponent<WorldItem>()
                          .ItemEntity.Path.ToLower()
                          .Contains("divinationcards")) && x.Item1 < 500);


            var tempList = orderedButOnlyCurrencyAndMap.Concat(orderedByDistance.Except(orderedButOnlyCurrencyAndMap))
                .ToList();


            var currentLabels = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                .GroupBy(y => y.ItemOnGround.Address)
                .ToDictionary(y => y.Key, y => y.First());
            ItemsOnGroundLabelElement entityLabel;

            if (tempList.Count == 0)
            {
                _Working = false;
                return;
            }


            foreach (var tuple in tempList)
                if (currentLabels.TryGetValue(tuple.Item2, out entityLabel))
                {

                    if (entityLabel.IsVisible)
                    {
                        var rect = entityLabel.Label.GetClientRect();
                        var vect = new Vector2(rect.Center.X, rect.Center.Y);
                        if (tuple.Item1 >= 750)
                        {
                            _Working = false;
                            return;
                        }
                        Thread.Sleep(5);
                        var vectWindow = GameController.Window.GetWindowRectangle();
                        if (vect.Y > vectWindow.Bottom || vect.Y < vectWindow.Top)
                        {
                            _Working = false;
                            return;
                        }
                        if (vect.X > vectWindow.Right || vect.X < vectWindow.Left)
                        {
                            _Working = false;
                            return;
                        }

                        _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                        Mouse.SetCursorPosAndLeftClick(vect + _clickWindowOffset, Settings.ExtraDelay);
                        break;
                    }
                }


            _Working = false;
        }

        List<Tuple<int, ItemsOnGroundLabelElement>> _currentLabels;

        private void NewPickUp()
        {
            if (pickUpTimer.ElapsedMilliseconds < Settings.PickupTimerDelay)
            {
                _Working = false;
                return;
            }
            pickUpTimer.Restart();
            if (_currentLabels == null || cacheTimer.ElapsedMilliseconds > Settings.CacheTimer)
            {
                _currentLabels = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                    .Where(x => x.ItemOnGround.Path.ToLower().Contains("worlditem") && x.IsVisible && x.CanPickUp)
                    .Select(x => new Tuple<int, ItemsOnGroundLabelElement>(GetEntityDistance(x.ItemOnGround), x))
                    .OrderBy(x => x.Item1)
                    .ToList();
                cacheTimer.Restart();
            }
            var pickUpThisItem = (from x in _currentLabels
                                     let lowerPath = x.Item2.ItemOnGround.GetComponent<WorldItem>()
                                         .ItemEntity.Path.ToLower()
                                     let sockets = x.Item2.ItemOnGround.GetComponent<WorldItem>()
                                         .ItemEntity.GetComponent<Sockets>()
                                     where lowerPath.Contains("currency") || lowerPath.Contains("divinationcards") ||
                                           lowerPath.Contains("map") ||
                                           sockets.NumberOfSockets == 6 && x.Item1 < Settings.PickupPriorityRange
                                     select x).FirstOrDefault() ?? _currentLabels.FirstOrDefault();
            if (pickUpThisItem != null)
            {
                if (pickUpThisItem.Item1 >= Settings.PickupRange)
                {
                    if (Settings.NoItemsClickChest)
                        ClickOnChests();
                    _Working = false;
                    return;
                }
                var vect = pickUpThisItem.Item2.Label.GetClientRect().Center;
                var vectWindow = GameController.Window.GetWindowRectangle();
                if (vect.Y + PIXEL_BORDER > vectWindow.Bottom || vect.Y - PIXEL_BORDER < vectWindow.Top)
                {
                    _Working = false;
                    return;
                }
                if (vect.X + PIXEL_BORDER > vectWindow.Right || vect.X - PIXEL_BORDER < vectWindow.Left)
                {
                    _Working = false;
                    return;
                }
                _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                var lastCursorPosition = Mouse.GetCursorPositionVector();
                Mouse.SetCursorPosAndLeftClick(vect + _clickWindowOffset, Settings.ExtraDelay);
                Mouse.SetCursorPos(lastCursorPosition);
            }
            else
            {
                if (Settings.NoItemsClickChest)
                    ClickOnChests();
            }
            _Working = false;
        }



        private void ClickOnChests()
        {
            var sortedByDistChest = new List<Tuple<int, long, EntityWrapper>>();

            foreach (var entity in entities)
            {
                if (entity.Path.ToLower().Contains("chests") && entity.IsAlive && entity.IsHostile)
                {
                    if (!entity.HasComponent<Chest>()) continue;
                    var ch = entity.GetComponent<Chest>();
                    if (ch.IsStrongbox) continue;
                    if (ch.IsOpened) continue;
                    var d = GetEntityDistance(entity);

                    var t = new Tuple<int, long, EntityWrapper>(d, entity.Address, entity);
                    if (sortedByDistChest.Any(x => x.Item2 == entity.Address)) continue;

                    sortedByDistChest.Add(t);
                }
            }

            var tempList = sortedByDistChest.OrderBy(x => x.Item1).ToList();
            if (tempList.Count <= 0) return;
                if (tempList[0].Item1 >= Settings.ChestRange) return;
                SetCursorToEntityAndClick(tempList[0].Item3);
            var centerScreen = GameController.Window.GetWindowRectangle().Center;
            Mouse.SetCursorPos(centerScreen);

            _Working = false;

        }
        //Copy-Paste - Sithylis_QoL
        private void SetCursorToEntityAndClick(EntityWrapper entity)
        {
            var camera = GameController.Game.IngameState.Camera;
            var chestScreenCoords =
                camera.WorldToScreen(entity.Pos.Translate(0, 0, 0), entity);
            if (chestScreenCoords != new Vector2())
            {
                var pos = Mouse.GetCursorPosition();
                var iconRect1 = new Vector2(chestScreenCoords.X, chestScreenCoords.Y);
                Mouse.SetCursorPosAndLeftClick(iconRect1, 100);
                Mouse.SetCursorPos(pos.X, pos.Y);

            }
        }

        #endregion



        #region SortMethodV4

        private void SortItems()
        {
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var items = MainSort();
            Action<Vector2, int> ClickMethod;
            if (Settings.SortLikeHuman)
                ClickMethod = Mouse.SetCursorPosAndLeftClickHuman;
            else
                ClickMethod = Mouse.SetCursorPosAndLeftClick;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (stash.Address != GameController.Game.IngameState.ServerData.StashPanel.VisibleStash.Address)
                {
                    _Working = false;
                    return;
                }
                if (item.nX == item.oX && item.nY == item.oY) continue;
                if (item.nX <0 || item.nY <0 || item.i<0 ) continue;
                var newVect = GetInventoryClickPosByCellIndex(stash, item.nX, item.nY);
                var oldVect = GetInventoryClickPosByCellIndex(stash, item.oX, item.oY);
                ClickOnItem(oldVect,newVect, ClickMethod);
                var movedItem = items.Where(x => x.oX == item.nX && x.oY == item.nY).ToList();
                if (movedItem.Any())
                {
                  var movedItemInList =  items[movedItem.First().i];
                    movedItemInList.oX = item.oX;
                    movedItemInList.oY = item.oY;
                }
                item.oX = item.nX;
                item.oY = item.nY;

            }
            Thread.Sleep(250);
            //This fix for item on cursor
            while (FIXGetHoldItemStatus() > 0)
            {
                Thread.Sleep(250);
                var xyFree = GetFirstFreeSpace();
                if (xyFree != null)
                {
                    var fixVect = GetInventoryClickPosByCellIndex(stash, (int) xyFree.Value.X, (int) xyFree.Value.Y);
                    ClickOnItem(fixVect, ClickMethod);
                }
            }
            _Working = false;




        }

        #endregion

        #region Usefull function

        public void ClickOnItem(Vector2 vector, Action<Vector2, int> _click)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            _click?.Invoke(vector+_clickWindowOffset,Settings.ExtraDelay);
        }

        public void ClickOnItemWShift(NormalInventoryItem item)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var cellWOff = item.GetClientRect().Center + _clickWindowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            Thread.Sleep(INPUT_DELAY);
            Mouse.SetCursorPosAndLeftClick(cellWOff, Settings.ExtraDelay);
            Thread.Sleep(INPUT_DELAY);
            Keyboard.KeyUp(Keys.LControlKey);
        }
        public void ClickOnItemWShift(ItemQ item)
        {
            ClickOnItemWShift(item.ItemData);
        }


        private void ClickOnItemWShift(ItemData item)
        {
            ClickOnItemWShift(item._inventoryItem);
        }
        private void ClickOnItem(Vector2 oldPosition, Vector2 newPosition, Action<Vector2, int> _click)
        {

            if (_click == null) return;
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            _click(oldPosition + _clickWindowOffset, Settings.ExtraDelay);
            Thread.Sleep(INPUT_DELAY);
            _click(newPosition + _clickWindowOffset, Settings.ExtraDelay);
            Thread.Sleep(INPUT_DELAY);
            _click(oldPosition + _clickWindowOffset, Settings.ExtraDelay);
        }



        private void ClickOnItem(ItemData item, Vector2 newPosition, Action<Vector2, int> _click)
        {
            ClickOnItem(item.GetClickPos(), newPosition, _click);
        }
        private void ClickOnItem(ItemQ item, Vector2 newPosition, Action<Vector2, int> _click)
        {
            ClickOnItem(item.ItemData.GetClickPos(), newPosition, _click);
        }
        public void ClickOnItemsWShift(IEnumerable<ItemQ> items)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                ClickOnItemWShift(item);
            }
        }

        public void ClickOnItemsWShift(IEnumerable<NormalInventoryItem> items)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                ClickOnItemWShift(item);
            }
        }
        private void ClickOnItemsWShift(IEnumerable<ItemData> itemsData)
        {

            foreach (var item in itemsData)
            {
                ClickOnItemWShift(item);
            }
        }
        private Vector2? GetFirstFreeSpace()
        {
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var items = stash.VisibleInventoryItems;
            var type = stash.InvType;
            bool[,] stsh;
            //Temp fix
          /*  if (type == InventoryType.QuadStash)
            {
                stsh = new bool[24, 24];
            }*/
            if (FIXGetSizeInventory() == 24)
                stsh = new bool[24, 24];
            else
                stsh = new bool[12, 12];
            var xMax = stsh.GetUpperBound(0);
            var yMax = stsh.GetUpperBound(1);
            foreach (var item in items)
            {
                if (item == null) continue;
                var baseItem = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                if(baseItem==null) continue;
                for (var i = 0; i < baseItem.Width; i++)
                for (var j = 0; j < baseItem.Height; j++)
                    stsh[item.InventPosX + i, item.InventPosY + j] = true;
            }


            for (var i = 0; i < xMax; i++)
            for (var j = 0; j < yMax; j++)
                if (!stsh[i, j]) return new Vector2(i, j);

            return null;
        }
        private List<ItemData> GetItemsDataFromInventoryItems(IEnumerable<NormalInventoryItem> items)
        {
            var itemDatas = new List<ItemData>();
            foreach (var item in items)
            {
                if (item.Item == null)
                    continue;

                var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                var testItem = new ItemData(item, baseItemType);
                itemDatas.Add(testItem);
            }
            return itemDatas;
        }


        private Vector2 GetInventoryClickPosByCellIndex(Inventory inventory, int indexX, int indexY)
        {
            var rectInv = inventory.InventoryUiElement.GetClientRect();
            var cellSize = rectInv.Width / 12;
            //Temp fix
           // if (inventory.InvType == InventoryType.QuadStash)
           if(FIXGetSizeInventory()==24)
                cellSize = rectInv.Width / 24;
            return rectInv.TopLeft +
                   new Vector2(cellSize * (indexX + 0.5f), cellSize * (indexY + 0.5f));
        }
        private int GetEntityDistance(EntityWrapper entity)
        {
            var PlayerPosition = GameController.Player.GetComponent<Positioned>();
            var MonsterPosition = entity.GetComponent<Positioned>();
            var distanceToEntity = Math.Sqrt(Math.Pow(PlayerPosition.X - MonsterPosition.X, 2) +
                                             Math.Pow(PlayerPosition.Y - MonsterPosition.Y, 2));

            return (int)distanceToEntity;
        }
        private int GetEntityDistance(Entity entity)
        {
            var PlayerPosition = GameController.Player.GetComponent<Positioned>();
            var MonsterPosition = entity.GetComponent<Positioned>();
            var distanceToEntity = Math.Sqrt(Math.Pow(PlayerPosition.X - MonsterPosition.X, 2) +
                                             Math.Pow(PlayerPosition.Y - MonsterPosition.Y, 2));

            return (int)distanceToEntity;
        }
        #endregion

        #region TempFix

        private long FIXGetSizeInventory()
        {
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            return Memory.ReadLong(stash.Address + 0x410, 0x630, 0x20); // Normal stash - 12, Quad - 24, Other 4 || 0
        }


        private int FIXGetHoldItemStatus()
        {
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            return  Memory.ReadInt(stash.Address + 0x410, 0x658); //0- No hold item 1 - FreeSpace, 2 - ReplaceItem
    }

        #endregion
    }
}