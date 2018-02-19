using System.Collections.Generic;
using StardewModdingAPI;

namespace JsonAssets.Data
{
    /// <summary>The metadata read from a content pack.</summary>
    internal class ContentPack
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The content pack name.</summary>
        public string Name { get; }

        /// <summary>A brief description of the content pack.</summary>
        public string Description { get; }

        /// <summary>The content pack author's name.</summary>
        public string Author { get; }

        /// <summary>The content pack version.</summary>
        public string Version { get; }

        /// <summary>The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</summary>
        public string[] UpdateKeys { get; }

        /// <summary>The added objects.</summary>
        public IList<ObjectData> Objects { get; } = new List<ObjectData>();

        /// <summary>The added crops.</summary>
        public IList<CropData> Crops { get; } = new List<CropData>();

        /// <summary>The added fruit trees.</summary>
        public IList<FruitTreeData> FruitTrees { get; } = new List<FruitTreeData>();

        /// <summary>The added big craftables.</summary>
        public IList<BigCraftableData> BigCraftables { get; } = new List<BigCraftableData>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The content pack name.</param>
        /// <param name="description">A brief description of the content pack.</param>
        /// <param name="author">The content pack author's name.</param>
        /// <param name="version">The content pack version.</param>
        /// <param name="updateKeys">The namespaced mod IDs to query for updates (like <c>Nexus:541</c>).</param>
        public ContentPack(string name, string description, string author, string version, string[] updateKeys)
        {
            this.Name = name;
            this.Description = description;
            this.Author = author;
            this.Version = version;
            this.UpdateKeys = updateKeys;
        }
    }
}
