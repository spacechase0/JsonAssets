using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using System.IO;
using JsonAssets.Data;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;

// TODO: Refactor recipes
// TODO: Handle recipe.IsDefault

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            MenuEvents.MenuChanged += menuChanged;
            SaveEvents.AfterLoad += afterLoad;

            // load content packs
            Log.info("Checking content packs...");
            foreach (ContentPack contentPack in this.GetContentPacks())
            {
                Log.info($"\t{contentPack.Name} {contentPack.Version} by {contentPack.Author} - {contentPack.Description}");

                foreach(ObjectData entry in contentPack.Objects)
                    this.objects.Add(entry);
                foreach(CropData entry in contentPack.Crops)
                    this.crops.Add(entry);
                foreach(FruitTreeData entry in contentPack.FruitTrees)
                    this.fruitTrees.Add(entry);
                foreach (BigCraftableData entry in contentPack.BigCraftables)
                    this.bigCraftables.Add(entry);
            }

            // assign IDs
            this.objectIds = this.AssignIds("objects", StartingObjectId, this.objects.ToList<DataNeedsId>());
            this.cropIds = this.AssignIds("crops", StartingCropId, this.crops.ToList<DataNeedsId>());
            this.fruitTreeIds = this.AssignIds("fruittrees", StartingFruitTreeId, this.fruitTrees.ToList<DataNeedsId>());
            this.bigCraftableIds = this.AssignIds("big-craftables", StartingBigCraftableId, this.bigCraftables.ToList<DataNeedsId>());

            // add content injector
            helper.Content.AssetEditors.Add(new ContentInjector());

        }

        private IEnumerable<ContentPack> GetContentPacks()
        {
            foreach (string dir in Directory.EnumerateDirectories(Path.Combine(Helper.DirectoryPath, "ContentPacks")))
            {
                // read manifest
                ContentPack contentPack;
                {
                    if (!File.Exists(Path.Combine(dir, "content-pack.json")))
                        continue;
                    var manifest = Helper.ReadJsonFile<LegacyManifest>(Path.Combine(dir, "content-pack.json"));
                    contentPack = new ContentPack(manifest.Name, manifest.Description, manifest.Author, manifest.Version, manifest.UpdateKeys.ToArray());
                }

                // read objects
                if (Directory.Exists(Path.Combine(dir, "Objects")))
                {
                    foreach (string objDir in Directory.EnumerateDirectories(Path.Combine(dir, "Objects")))
                    {
                        if (!File.Exists(Path.Combine(objDir, "object.json")))
                            continue;
                        string relativeDir = Path.Combine("ContentPacks", Path.GetFileName(dir), "Objects", Path.GetFileName(objDir));

                        ObjectData obj = Helper.ReadJsonFile<ObjectData>(Path.Combine(objDir, "object.json"));
                        obj.texture = Helper.Content.Load<Texture2D>($"{relativeDir}/object.png");
                        if ( obj.IsColored )
                            obj.textureColor = Helper.Content.Load<Texture2D>($"{relativeDir}/color.png");
                        
                        contentPack.Objects.Add(obj);
                    }
                }

                // read crops
                if (Directory.Exists(Path.Combine(dir, "Crops")))
                {
                    foreach (string cropDir in Directory.EnumerateDirectories(Path.Combine(dir, "Crops")))
                    {
                        if (!File.Exists(Path.Combine(cropDir, "crop.json")))
                            continue;
                        string relativeDir = Path.Combine("ContentPacks", Path.GetFileName(dir), "Crops", Path.GetFileName(cropDir));

                        var crop = Helper.ReadJsonFile<CropData>(Path.Combine(cropDir, "crop.json"));
                        crop.texture = Helper.Content.Load<Texture2D>($"{relativeDir}/crop.png");
                        contentPack.Crops.Add(crop);

                        var obj = new ObjectData
                        {
                            texture = Helper.Content.Load<Texture2D>($"{relativeDir}/seeds.png"),
                            Name = crop.SeedName,
                            Description = crop.SeedDescription,
                            Category = ObjectData.Category_.Seeds,
                            Price = crop.SeedPurchasePrice,
                            CanPurchase = true,
                            PurchaseFrom = crop.SeedPurchaseFrom,
                            PurchasePrice = crop.SeedPurchasePrice,
                            PurchaseRequirements = crop.SeedPurchaseRequirements ?? new List<string>()
                        };

                        string[] excludeSeasons = new [] { "spring", "summer", "fall", "winter" }.Except(crop.Seasons).ToArray();
                        var str = $"z {string.Join(" ", excludeSeasons)}".Trim();
                        obj.PurchaseRequirements.Add(str);

                        crop.seed = obj;

                        contentPack.Objects.Add(obj);
                    }
                }

                // read fruit trees
                if (Directory.Exists(Path.Combine(dir, "FruitTrees")))
                {
                    foreach (string fruitTreeDir in Directory.EnumerateDirectories(Path.Combine(dir, "FruitTrees")))
                    {
                        if (!File.Exists(Path.Combine(fruitTreeDir, "tree.json")))
                            continue;
                        string relativeDir = Path.Combine("ContentPacks", Path.GetFileName(dir), "FruitTrees", Path.GetFileName(fruitTreeDir));

                        FruitTreeData fruitTree = Helper.ReadJsonFile<FruitTreeData>(Path.Combine(fruitTreeDir, "tree.json"));
                        fruitTree.texture = Helper.Content.Load<Texture2D>($"{relativeDir}/tree.png");
                        contentPack.FruitTrees.Add(fruitTree);

                        ObjectData obj = new ObjectData
                        {
                            texture = Helper.Content.Load<Texture2D>($"{relativeDir}/sapling.png"),
                            Name = fruitTree.SaplingName,
                            Description = fruitTree.SaplingDescription,
                            Category = ObjectData.Category_.Seeds,
                            Price = fruitTree.SaplingPurchasePrice,
                            CanPurchase = true,
                            PurchaseFrom = fruitTree.SsaplingPurchaseFrom,
                            PurchasePrice = fruitTree.SaplingPurchasePrice
                        };
                        
                        fruitTree.sapling = obj;

                        contentPack.Objects.Add(obj);
                    }
                }

                // read bigcraftables
                if (Directory.Exists(Path.Combine(dir, "BigCraftables")))
                {
                    foreach (string bigDir in Directory.EnumerateDirectories(Path.Combine(dir, "BigCraftables")))
                    {
                        if (!File.Exists(Path.Combine(bigDir, "big-craftable.json")))
                            continue;
                        string relativeDir = Path.Combine("ContentPacks", Path.GetFileName(dir), "BigCraftables", Path.GetFileName(bigDir));

                        BigCraftableData bigInfo = Helper.ReadJsonFile<BigCraftableData>(Path.Combine(bigDir, "big-craftable.json"));
                        bigInfo.texture = Helper.Content.Load<Texture2D>($"{relativeDir}/big-craftable.png");
                        contentPack.BigCraftables.Add(bigInfo);
                    }
                }

                yield return contentPack;
            }
        }

        private void menuChanged(object sender, EventArgsClickableMenuChanged args)
        {
            var menu = args.NewMenu as ShopMenu;
            if (menu == null || menu.portraitPerson == null)
                return;

            //if (menu.portraitPerson.name == "Pierre")
            {
                Log.trace($"Adding objects to {menu.portraitPerson.name}'s shop");

                var forSale = Helper.Reflection.GetPrivateValue<List<Item>>(menu, "forSale");
                var itemPriceAndStock = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(menu, "itemPriceAndStock");

                var precondMeth = Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "checkEventPrecondition");
                foreach (var obj in objects)
                {
                    if ( obj.Recipe != null && obj.Recipe.CanPurchase )
                    {
                        if (obj.Recipe.PurchaseFrom != menu.portraitPerson.name)
                            continue;
                        if (Game1.player.craftingRecipes.ContainsKey(obj.Name) || Game1.player.cookingRecipes.ContainsKey(obj.Name))
                            continue;
                        if (obj.Recipe.PurchaseRequirements != null && obj.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { obj.Recipe.GetPurchaseRequirementString() }) == -1)
                            continue;
                        var recipeObj = new StardewValley.Object(obj.id, 1, true, obj.Recipe.PurchasePrice, 0);
                        forSale.Add(recipeObj);
                        itemPriceAndStock.Add(recipeObj, new int[] { obj.Recipe.PurchasePrice, 1 });
                        Log.trace($"\tAdding recipe for {obj.Name}");
                    }
                    if (!obj.CanPurchase)
                        continue;
                    if (obj.PurchaseFrom != menu.portraitPerson.name)
                        continue;
                    if (obj.PurchaseRequirements != null && obj.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { obj.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new StardewValley.Object(Vector2.Zero, obj.id, int.MaxValue);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { obj.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {obj.Name}");
                }
                foreach (var big in bigCraftables)
                {
                    if (big.Recipe != null && big.Recipe.CanPurchase)
                    {
                        if (big.Recipe.PurchaseFrom != menu.portraitPerson.name)
                            continue;
                        if (Game1.player.craftingRecipes.ContainsKey(big.Name) || Game1.player.cookingRecipes.ContainsKey(big.Name))
                            continue;
                        if (big.Recipe.PurchaseRequirements != null && big.Recipe.PurchaseRequirements.Count > 0 &&
                            precondMeth.Invoke<int>(new object[] { big.Recipe.GetPurchaseRequirementString() }) == -1)
                            continue;
                        var recipeObj = new StardewValley.Object(new Vector2(0, 0), big.id, true);
                        forSale.Add(recipeObj);
                        itemPriceAndStock.Add(recipeObj, new int[] { big.Recipe.PurchasePrice, 1 });
                        Log.trace($"\tAdding recipe for {big.Name}");
                    }
                    if (!big.CanPurchase)
                        continue;
                    if (big.PurchaseFrom != menu.portraitPerson.name)
                        continue;
                    if (big.PurchaseRequirements != null && big.PurchaseRequirements.Count > 0 &&
                        precondMeth.Invoke<int>(new object[] { big.GetPurchaseRequirementString() }) == -1)
                        continue;
                    Item item = new StardewValley.Object(Vector2.Zero, big.id, false);
                    forSale.Add(item);
                    itemPriceAndStock.Add(item, new int[] { big.PurchasePrice, int.MaxValue });
                    Log.trace($"\tAdding {big.Name}");
                }
            }
        }

        private void afterLoad( object sender, EventArgs args )
        {
            foreach ( var obj in objects )
            {
                if ( obj.Recipe != null && obj.Recipe.IsDefault && !Game1.player.knowsRecipe(obj.Name) )
                {
                    if ( obj.Category == ObjectData.Category_.Cooking )
                    {
                        Game1.player.cookingRecipes.Add(obj.Name, 0);
                    }
                    else
                    {
                        Game1.player.cookingRecipes.Add(obj.Name, 0);
                    }
                }
            }
        }

        private const int StartingObjectId = 2000;
        private const int StartingCropId = 100;
        private const int StartingFruitTreeId = 20;
        private const int StartingBigCraftableId = 300;
        internal IList<ObjectData> objects = new List<ObjectData>();
        internal IList<CropData> crops = new List<CropData>();
        internal IList<FruitTreeData> fruitTrees = new List<FruitTreeData>();
        internal IList<BigCraftableData> bigCraftables = new List<BigCraftableData>();
        private IDictionary<string, int> objectIds;
        private IDictionary<string, int> cropIds;
        private IDictionary<string, int> fruitTreeIds;
        private IDictionary<string, int> bigCraftableIds;

        public int ResolveObjectId( object data )
        {
            if (data.GetType() == typeof(long))
                return (int)(long)data;
            else
                return objectIds[ (string) data ];
        }

        private Dictionary<string, int> AssignIds( string type, int starting, IList<DataNeedsId> data )
        {
            var saved = Helper.ReadJsonFile<Dictionary<string, int>>(Path.Combine(Helper.DirectoryPath,$"ids-{type}.json"));
            Dictionary<string, int> ids = new Dictionary<string, int>();

            int currId = starting;
            // First, populate saved IDs
            foreach ( var d in data )
            {
                if (saved != null && saved.ContainsKey(d.Name))
                {
                    ids.Add(d.Name, saved[d.Name]);
                    currId = Math.Max(currId, saved[d.Name] + 1);
                    d.id = ids[d.Name];
                }
            }
            // Next, add in new IDs
            foreach (var d in data)
            {
                if (d.id == -1)
                {
                    ids.Add(d.Name, currId++);
                    if (type == "objects" && ((ObjectData)d).IsColored)
                        ++currId;
                    d.id = ids[d.Name];
                }
            }

            Helper.WriteJsonFile(Path.Combine(Helper.DirectoryPath, $"ids-{type}.json"), ids);
            return ids;
        }
    }
}
