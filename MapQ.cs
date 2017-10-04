using System.Collections.Generic;
using System.Linq;


namespace QvinExp
{
   public class MapQ

    {
        public string Name { get; }
        public int Tier { get; }
        public ItemData ItemData { get; }
        public int nX { get; set; } = -1;
        public int nY { get; set; } = -1;

        public MapQ(string name, int tier, ItemData itemData)
        {
            this.Name = name;
            this.Tier = tier;
            this.ItemData = itemData;
        }


        public MapQ(string name, ItemData itemData)
        {
            this.Name = name;
            this.Tier = Maps().FirstOrDefault(x => x.Key == name).Value;
            this.ItemData = itemData;
        }


        public static Dictionary<string, int> Maps()
        {
            var maps = new Dictionary<string, int>
            {
                {"Arcade Map", 1},
                {"Desert Map", 1},
                {"Crystal Ore Map", 1},
                {"Jungle Valley Map", 1},
                {"Beach Map", 2},
                {"Factory Map", 2},
                {"Ghetto Map", 2},
                {"Oasis Map", 2},
                {"Arid Lake Map", 3},
                {"Cavern Map", 3},
                {"Channel Map", 3},
                {"Grotto Map", 3},
                {"Marshes Map", 3},
                {"Sewer Map", 3},
                {"Vaal Pyramid Map", 3},
                {"Academy Map", 4},
                {"Acid Lakes Map", 4},
                {"Dungeon Map", 4},
                {"Graveyard Map", 4},
                {"Phantasmagoria Map", 4},
                {"Villa Map", 4},
                {"Waste Pool Map", 4},
                {"Burial Chambers Map", 5},
                {"Mesa Map", 5},
                {"Dunes Map", 5},
                {"Peninsula Map", 5},
                {"Pit Map", 5},
                {"Primordial Pool Map", 5},
                {"Spider Lair Map", 5},
                {"Tower Map", 5},
                {"Canyon Map", 6},
                {"Quarry Map", 6},
                {"Racecourse Map", 6},
                {"Ramparts Map", 6},
                {"Spider Forest Map", 6},
                {"Strand Map", 6},
                {"Thicket Map", 6},
                {"Vaal City Map", 6},
                {"Wharf Map", 6},
                {"Arachnid Tomb Map", 7},
                {"Armoury Map", 7},
                {"Ashen Wood Map", 7},
                {"Castle Ruins Map", 7},
                {"Catacombs Map", 7},
                {"Cells Map", 7},
                {"Mud Geyser Map", 7},
                {"Arachnid Nest Map", 8},
                {"Arena Map", 8},
                {"Atoll Map", 8},
                {"Barrows Map", 8},
                {"Bog Map", 8},
                {"Cemetery Map", 8},
                {"Pier Map", 8},
                {"Shore Map", 8},
                {"Tropical Island Map", 8},
                {"Coves Map", 9},
                {"Crypt Map", 9},
                {"Museum Map", 9},
                {"Orchard Map", 9},
                {"Overgrown Shrine Map", 9},
                {"Promenade Map", 9},
                {"Reef Map", 9},
                {"Temple Map", 9},
                {"Arsenal Map", 10},
                {"The Beachhead", 10},
                {"Colonnade Map", 10},
                {"Courtyard Map", 10},
                {"Malformation Map", 10},
                {"Port Map", 10},
                {"Terrace Map", 10},
                {"Underground River Map", 10},
                {"Bazaar Map", 11},
                {"Chateau Map", 11},
                {"Excavation Map", 11},
                {"Precinct Map", 11},
                {"Torture Chamber Map", 11},
                {"Underground Sea Map", 11},
                {"Wasteland Map", 11},
                {"Crematorium Map", 12},
                {"Estuary Map", 12},
                {"Ivory Temple Map", 12},
                {"Necropolis Map", 12},
                {"Plateau Map", 12},
                {"Residence Map", 12},
                {"Shipyard Map", 12},
                {"Vault Map", 12},
                {"Beacon Map", 13},
                {"Gorge Map", 13},
                {"High Gardens Map", 13},
                {"Lair Map", 13},
                {"Plaza Map", 13},
                {"Scriptorium Map", 13},
                {"Sulphur Wastes Map", 13},
                {"Waterways Map", 13},
                {"Maze Map", 14},
                {"Mineral Pools Map", 14},
                {"Palace Map", 14},
                {"Shrine Map", 14},
                {"Springs Map", 14},
                {"Volcano Map", 14},
                {"Abyss Map", 15},
                {"Colosseum Map", 15},
                {"Core Map", 15},
                {"Dark Forest Map", 15},
                {"Overgrown Ruin Map", 15},
                {"Shaped Jungle Valley Map", 6},
                {"Shaped Beach Map", 7},
                {"Shaped Acid Lakes Map", 9},
                {"Shaped Arid Lake Map", 8},
                {"Shaped Mesa Map", 10},
                {"Shaped Racecourse Map", 11},
                {"Shaped Spider Forest Map", 11},
                {"Shaped Ashen Wood Map", 12},
                {"Shaped Shore Map", 13},
                {"Shaped Bog Map", 13},
                {"Shaped Coves Map", 14},
                {"Offering to the Goddess", 99},
                {"Harbinger Map", 17}
            };




            return maps;
        }

    }

}
