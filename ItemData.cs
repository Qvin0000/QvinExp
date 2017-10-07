#region Header

//-----------------------------------------------------------------
//   Class:          ItemData
//   Description:    Input item data for filter
//   Author:         Stridemann        Date: 08.26.2017
//-----------------------------------------------------------------

#endregion

using PoeHUD.Models;
using PoeHUD.Models.Enums;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.Elements;
using SharpDX;

namespace QvinExp
{
    public class ItemData
    {
        public readonly NormalInventoryItem _inventoryItem;

        public string Path { get;  }
        public string ClassName { get; }
        public string BaseName { get;  }
        public ItemRarity Rarity { get;  }
        public int ItemQuality { get;  }
        public bool BIdentified { get; }
        public int ItemLevel {  get;  }

        public ItemData(NormalInventoryItem inventoryItem, BaseItemType baseItemType)
        {
            _inventoryItem = inventoryItem;
            var item = inventoryItem.Item;
            Path = item.Path;

            var mods = item.GetComponent<Mods>();
            Rarity = mods.ItemRarity;
            BIdentified = mods.Identified;
            ItemLevel = mods.ItemLevel;
            var quality = item.GetComponent<Quality>();
            ItemQuality = quality.ItemQuality;
            ClassName = baseItemType.ClassName;
            BaseName = baseItemType.BaseName;
        }

        public Vector2 GetClickPos()
        {
            return _inventoryItem.GetClientRect().Center;
        }
    }
}