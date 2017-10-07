using System.Collections.Generic;
using System.Linq;


namespace QvinExp
{
   public class ItemQ

    {

        public int Tier { get; } 
        public ItemData ItemData { get; }
        public int nX { get; set; } = -1;
        public int nY { get; set; } = -1;
        public int oX { get; set; } 
        public int oY { get; set; }
        public int i { get; set; } = -1;
          
        public ItemQ(int tier, ItemData itemData)
        {
            this.Tier = tier;
            this.ItemData = itemData;
            this.oX = itemData._inventoryItem.InventPosX;
            this.oY = itemData._inventoryItem.InventPosY;
        }


        public ItemQ(ItemData itemData)
        {
            this.ItemData = itemData;
            this.oX = itemData._inventoryItem.InventPosX;
            this.oY = itemData._inventoryItem.InventPosY;
        }




    }

}
