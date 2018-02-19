﻿using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace JsonAssets.Data
{
    public class ObjectData : DataNeedsId
    {
        [JsonIgnore]
        internal Texture2D texture;
        [JsonIgnore]
        internal Texture2D textureColor;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category_
        {
            Vegetable = SObject.VegetableCategory,
            Fruit = SObject.FruitsCategory,
            Flower = SObject.flowersCategory,
            Gem = SObject.GemCategory,
            Fish = SObject.FishCategory,
            Egg = SObject.EggCategory,
            Milk = SObject.MilkCategory,
            Cooking = SObject.CookingCategory,
            Crafting = SObject.CraftingCategory,
            Mineral = SObject.mineralsCategory,
            Meat = SObject.meatCategory,
            Metal = SObject.metalResources,
            Junk = SObject.junkCategory,
            Syrup = SObject.syrupCategory,
            MonsterLoot = SObject.monsterLootCategory,
            ArtisanGoods = SObject.artisanGoodsCategory,
            Seeds = SObject.SeedsCategory,
        }

        public class Recipe_
        {
            public class Ingredient
            {
                public object Object { get; set; }
                public int Count { get; set; }
            }
            // Possibly friendship option (letters, like vanilla) and/or skill levels (on levelup?)
            public int ResultCount { get; set; } = 1;
            public IList<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

            public bool IsDefault { get; set; } = false;
            public bool CanPurchase { get; set; } = false;
            public int PurchasePrice { get; set; }
            public string PurchaseFrom { get; set; } = "Gus";
            public IList<string> PurchaseRequirements { get; set; } = new List<string>();

            internal string GetRecipeString( ObjectData parent )
            {
                var str = "";
                foreach (var ingredient in Ingredients)
                    str += Mod.instance.ResolveObjectId(ingredient.Object) + " " + ingredient.Count + " ";
                str = str.Substring(0, str.Length - 1);
                str += $"/what is this for?/{parent.id}/";
                if (parent.Category != Category_.Cooking)
                    str += "false/";
                str += "/null"; // TODO: Requirement
                return str;
            }

            internal string GetPurchaseRequirementString()
            {
                var str = $"1234567890";
                foreach (var cond in PurchaseRequirements)
                    str += $"/{cond}";
                return str;
            }
        }

        public class FoodBuffs_
        {
            public int Farming { get; set; } = 0;
            public int Fishing { get; set; } = 0;
            public int Mining { get; set; } = 0;
            public int Luck { get; set; } = 0;
            public int Foraging { get; set; } = 0;
            public int MaxStamina { get; set; } = 0;
            public int MagnetRadius { get; set; } = 0;
            public int Speed { get; set; } = 0;
            public int Defense { get; set; } = 0;
            public int Attack { get; set; } = 0;
            public int Duration { get; set; } = 0;
        }
        
        public string Description { get; set; }
        public Category_ Category { get; set; }
        public bool IsColored { get; set; } = false;

        public int Price { get; set; }

        public Recipe_ Recipe { get; set; }

        public int Edibility { get; set; } = SObject.inedible;
        public bool EdibleIsDrink { get; set; } = false;
        public FoodBuffs_ EdibleBuffs = new FoodBuffs_();

        public bool CanPurchase { get; set; } = false;
        public int PurchasePrice { get; set; }
        public string PurchaseFrom { get; set; } = "Pierre";
        public IList<string> PurchaseRequirements { get; set; } = new List<string>();

        // TODO: Gift taste overrides.

        public int GetObjectId() { return id; }

        internal string GetObjectInformation()
        {
            if (Category == Category_.Cooking)
            {
                var str = $"{Name}/{Price}/{Edibility}/Cooking -7/{Name}/{Description}/";
                str += (EdibleIsDrink ? "drink" : "food") + "/";
                if (EdibleBuffs == null)
                    EdibleBuffs = new FoodBuffs_();
                str += $"{EdibleBuffs.Farming} {EdibleBuffs.Fishing} {EdibleBuffs.Mining} 0 {EdibleBuffs.Luck} {EdibleBuffs.Foraging} 0 {EdibleBuffs.MaxStamina} {EdibleBuffs.MagnetRadius} {EdibleBuffs.Speed} {EdibleBuffs.Defense} {EdibleBuffs.Attack}/{EdibleBuffs.Duration}";
                return str;
            }
            else
            {
                var itype = (int)Category;
                return $"{Name}/{Price}/{Edibility}/Basic {itype}/{Name}/{Description}";
            }
        }

        internal string GetPurchaseRequirementString()
        {
            var str = $"1234567890";
            foreach (var cond in PurchaseRequirements)
                str += $"/{cond}";
            return str;
        }
    }
}
