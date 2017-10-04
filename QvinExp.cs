using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using PoeHUD.DebugPlug;
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
        private readonly bool _DebugInFile = true;
        private int[,] _ignoredCells;
        private bool _Working;
        private readonly List<EntityWrapper> entities = new List<EntityWrapper>();
        private readonly int INPUT_DELAY = 15;
        private readonly int UPDATE_DELAY = 256;
        private bool GemsOnOff = false; 
        public override void Initialise()
        {
            LoadIgnoredCells();
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
        }

        public override void Render()
        {
            if (Keyboard.IsKeyDown((int) Keys.F1))
            {
                if (_Working)
                    return;
                _Working = true;
                TestPickUp();
            }

            if (GemsOnOff)
            {
                if (Keyboard.IsKeyDown((int) Keys.F6))
                {
                    if (_Working)
                        return;
                    _Working = true;
                    GetGems();
                }
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
                //SortItemsInStash();
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

        private List<ItemData> GetItemsDataFromInventoryItems(List<NormalInventoryItem> items)
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


        private List<MapQ> SortMaps(List<NormalInventoryItem> items)
        {
            var itemsData = GetItemsDataFromInventoryItemsForMap(items);
            var maps = new List<MapQ>();
            var allMaps = MapQ.Maps();
            foreach (var itemData in itemsData)
            {
                var tier = 99;
                if (allMaps.ContainsKey(itemData.BaseName))
                    tier = allMaps[itemData.BaseName];
                maps.Add(new MapQ(itemData.BaseName, tier, itemData));
            }

            var result = maps.OrderBy(x => x.Tier)
                .ThenBy(y => y.Name)
                .ThenBy(u => u.ItemData.Rarity)
                .ThenBy(q => q.ItemData._inventoryItem.InventPosX)
                .ToList();
            return result;
        }

        private List<MapQ> SortMapsNew(List<NormalInventoryItem> items)
        {
            var itemsData = GetItemsDataFromInventoryItemsForMap(items);
            var maps = new List<MapQ>();
            var allMaps = MapQ.Maps();
            foreach (var itemData in itemsData)
            {
                var tier = 99;
                tier = itemData._inventoryItem.Item.GetComponent<Map>().Tier;
                maps.Add(new MapQ(itemData.BaseName, tier, itemData));
            }

            var result = maps.OrderBy(x => x.Tier)
                .ThenBy(y => y.Name)
                .ThenBy(u => u.ItemData.Rarity)
                .ThenBy(q => q.ItemData._inventoryItem.InventPosX)
                .ToList();
            return result;
        }
       
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

        private List<ItemData> GetItemsDataFromInventoryItemsForMap(List<NormalInventoryItem> items)
        {
            var itemDatas = new List<ItemData>();
            foreach (var item in items)
            {
                if (item.Item == null)
                    continue;


                if (!item.Item.Path.ToLower().Contains("maps") &&
                    !item.Item.Path.Contains("Metadata/Items/Labyrinth/OfferingToTheGoddess")) continue;
                var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                var testItem = new ItemData(item, baseItemType);
                itemDatas.Add(testItem);
            }
            return itemDatas;
        }

        private Vector2 GetInventoryClickPosByCellIndex(Inventory inventory, int indexX, int indexY, float cellSize)
        {
            var rectInv = inventory.InventoryUiElement.GetClientRect();
            cellSize = rectInv.Width / 12;
            if(inventory.InvType == InventoryType.QuadStash)
                cellSize = rectInv.Width / 24;
            return rectInv.TopLeft +
                   new Vector2(cellSize * (indexX + 0.5f), cellSize * (indexY + 0.5f));
        }

        private void Get3Maps(int howMany = 48)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var stashItems = stash.VisibleInventoryItems.ToList();
            var sortedMaps = SortMaps(stashItems);
            var grouped = sortedMaps.GroupBy(x => x.Name);
            var moreThan3Map = new List<MapQ>();
            foreach (var gr in grouped)
            {
                if (gr.Count() < 3) continue;
                var mapsCount = gr.Count();
                var sellCount = (int) Math.Floor(mapsCount / 3.0) * 3;
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

        #region Debug

        private void Debug(string message, int time = 1)
        {
            if (_Debug)
            {
                DebugPlugin.LogMsg(message, time);
                if (_DebugInFile)
                    File.AppendAllText("Debug.txt", message + Environment.NewLine);
            }
        }

        #endregion

        #region NewSortMethod

        public void ClickOnMap(MapQ map)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var cellWOff = map.ItemData.GetClickPos() + _clickWindowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            Thread.Sleep(INPUT_DELAY);
            Mouse.SetCursorPosAndLeftClick(cellWOff, Settings.ExtraDelay);
            Thread.Sleep(INPUT_DELAY);
            Keyboard.KeyUp(Keys.LControlKey);
        }


        public void ClickOnItem(NormalInventoryItem item)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var cellWOff = item.GetClientRect().Center + _clickWindowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            Thread.Sleep(INPUT_DELAY);
            Mouse.SetCursorPosAndLeftClick(cellWOff, Settings.ExtraDelay);
            Thread.Sleep(INPUT_DELAY);
            Keyboard.KeyUp(Keys.LControlKey);
        }

        private void ClickOnItemData(ItemData item)
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            if (item == null) return;
            var cellWOff = item.GetClickPos() + _clickWindowOffset;
            Keyboard.KeyDown(Keys.LControlKey);
            Thread.Sleep(INPUT_DELAY);
            Mouse.SetCursorPosAndLeftClick(cellWOff, Settings.ExtraDelay);
            Thread.Sleep(INPUT_DELAY);
            Keyboard.KeyUp(Keys.LControlKey);
        }

        public void ClickOnMaps(IEnumerable<MapQ> maps)
        {
            foreach (var map in maps)
            {
                if (map == null) continue;
                ClickOnMap(map);
            }
        }

        public void ClickOnItems(IEnumerable<NormalInventoryItem> items)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                ClickOnItem(item);
            }
        }
        private void ClickOnItemsData(List<ItemData> itemsData)
        {
            
            foreach (var item in itemsData)
            {
                ClickOnItemData(item);
            }
        }

        public void ClickOnMaps(IEnumerable<NormalInventoryItem> items)
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                ClickOnItem(item);
            }
        }

        public List<NormalInventoryItem> SortMapsByInvPosition(IEnumerable<NormalInventoryItem> items)
        {
            var result = items.OrderBy(x => x.InventPosX).ThenBy(x => x.InventPosY).ToList();
            return result;
        }


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
                var sortedMaps = SortMapsNew(cacheItems);
                var finalSorted = sortedMaps.Skip(i * sizeSortingHalf).Take(sizeSortingHalf).ToList();
                ClickOnMaps(finalSorted);
                var sortedMapsByInvPosition = SortMapsByInvPosition(cacheItems)
                    .Skip(i * sizeSortingHalf)
                    .Take(sizeSortingHalf)
                    .ToList();
                Thread.Sleep((int) (UPDATE_DELAY * 1.5));
                var sortedMapsByInvPosition2 = SortMapsByInvPosition(stash.VisibleInventoryItems.ToList()).ToList();
                var Interct = sortedMapsByInvPosition2.Intersect(sortedMapsByInvPosition);
                ClickOnMaps(Interct);
                Thread.Sleep(UPDATE_DELAY);
                var sortedMapsFromInventory = SortMapsNew(inventory.VisibleInventoryItems.ToList());
                ClickOnMaps(sortedMapsFromInventory);
                Thread.Sleep(UPDATE_DELAY);
            }
            _Working = false;
        }

        #endregion

        #region pickup

        private readonly Stopwatch pickUpTimer = Stopwatch.StartNew();

        private readonly List<Tuple<int, long, EntityWrapper>> SortedByDistDropItems =
            new List<Tuple<int, long, EntityWrapper>>();

        private void TestPickUp()
        {
            if (pickUpTimer.ElapsedMilliseconds < 100)
            {
                _Working = false;
                return;
            }
            pickUpTimer.Restart();
            SortedByDistDropItems.Clear();


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
                SortedByDistDropItems.Add(t);
            }


            var OrderedByDistance = SortedByDistDropItems.OrderBy(x => x.Item1).ToList();

            var OrderedButOnlyCurrencyAndMap = OrderedByDistance.Where(
                x => (x.Item3.GetComponent<WorldItem>().ItemEntity.Path.ToLower().Contains("currency") ||
                      x.Item3.GetComponent<WorldItem>().ItemEntity.Path.ToLower().Contains("map") || x.Item3
                          .GetComponent<WorldItem>()
                          .ItemEntity.Path.ToLower()
                          .Contains("divinationcards")) && x.Item1 < 500);


            var tempList = OrderedButOnlyCurrencyAndMap.Concat(OrderedByDistance.Except(OrderedButOnlyCurrencyAndMap))
                .ToList();


            var _currentLabels = GameController.Game.IngameState.IngameUi.ItemsOnGroundLabels
                .GroupBy(y => y.ItemOnGround.Address)
                .ToDictionary(y => y.Key, y => y.First());
            ItemsOnGroundLabelElement entityLabel;

            if (tempList.Count == 0)
            {
                _Working = false;
                return;
            }

            foreach (var tuple in tempList)
                if (_currentLabels.TryGetValue(tuple.Item2, out entityLabel))
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

                        SetCursorToEntityAndClick(vect);
                        break;
                    }


            _Working = false;
        }

        private int GetEntityDistance(EntityWrapper entity)
        {
            var PlayerPosition = GameController.Player.GetComponent<Positioned>();
            var MonsterPosition = entity.GetComponent<Positioned>();
            var distanceToEntity = Math.Sqrt(Math.Pow(PlayerPosition.X - MonsterPosition.X, 2) +
                                             Math.Pow(PlayerPosition.Y - MonsterPosition.Y, 2));

            return (int) distanceToEntity;
        }

        private void SetCursorToEntityAndClick(Vector2 rect)
        {
            var _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var finalRect = rect + _clickWindowOffset;
            Mouse.SetCursorPosAndLeftClick(finalRect, 30);
        }

        #endregion

        #region Gems?
        private void GetGems()
        {
            _clickWindowOffset = GameController.Window.GetWindowRectangle().TopLeft;
            var stash = GameController.Game.IngameState.ServerData.StashPanel.VisibleStash;
            var itemsData = GetItemsDataFromInventoryItems(stash.VisibleInventoryItems);
            /*
            var activeSkillGems = itemsData.Where(x => x.ClassName.ToLower().Contains("Active Skill Gem".ToLower()));
            var supportSkillGems = itemsData.Where(x => x.ClassName.ToLower().Contains("Support Skill Gem".ToLower()));
            var activeSorted = activeSkillGems.OrderBy(x => x.ItemQuality).ToList();
            var supportSorted = supportSkillGems.OrderBy(x => x.ItemQuality).ToList();
            */

            /*
            List<ItemData> final = new List<ItemData>();
            
            final.AddRange(activeSorted);
            final.AddRange(supportSorted);
            */
            var skillGems = itemsData.Where(x => x.ClassName.ToLower().Contains("Skill Gem".ToLower()));
            var final = skillGems.OrderBy(x => x.ItemQuality).ToList();
            final = final.Take(50).ToList();
            ClickOnItemsData(final);
            _Working = false;
        }
        #endregion
    }
}