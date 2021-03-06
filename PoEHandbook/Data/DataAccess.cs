﻿//  ------------------------------------------------------------------ 
//  PoEHandbook
//  DataAccess.cs by Tyrrrz
//  26/04/2015
//  ------------------------------------------------------------------ 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using PoEHandbook.Model;

namespace PoEHandbook.Data
{
    public static class DataAccess
    {
        private static readonly HashSet<Entity> Entities = new HashSet<Entity>();

        private static void LoadEntities()
        {
            Entities.Clear();
            var resources = new Dictionary<string, Type>
            {
                {"misc.xml", typeof (Entity)},

                {"recipes.xml", typeof (Recipe)},

                {"passives.xml", typeof (Passive)},

                {"currency.xml", typeof (Currency)},

                {"unique_maps.xml", typeof (Map)},

                {"unique_jewels.xml", typeof (Jewel)},

                {"unique_flasks.xml", typeof (Flask)},

                {"unique_body_armours.xml", typeof (Armour)},
                {"unique_helmets.xml", typeof (Armour)},
                {"unique_gloves.xml", typeof (Armour)},
                {"unique_boots.xml", typeof (Armour)},

                {"unique_amulets.xml", typeof (Accessory)},
                {"unique_belts.xml", typeof (Accessory)},
                {"unique_rings.xml", typeof (Accessory)},
                {"unique_quivers.xml", typeof (Accessory)},

                {"unique_shields.xml", typeof (Shield)},

                {"unique_axes.xml", typeof (Weapon)},
                {"unique_bows.xml", typeof (Weapon)},
                {"unique_claws.xml", typeof (Weapon)},
                {"unique_daggers.xml", typeof (Weapon)},
                {"unique_rods.xml", typeof (Weapon)},
                {"unique_maces.xml", typeof (Weapon)},
                {"unique_swords.xml", typeof (Weapon)},
                {"unique_staves.xml", typeof (Weapon)},
                {"unique_wands.xml", typeof (Weapon)}
            };

            foreach (var resource in resources)
            {
                string path = Path.Combine(Environment.CurrentDirectory, "Data", resource.Key);

                if (!File.Exists(path)) continue;

                var doc = new XmlDocument();
                doc.Load(path);

                var entityNodes = doc.SelectNodes("Root/Entity");
                if (entityNodes == null) continue;

                foreach (XmlNode node in entityNodes)
                {
                    var item = (Entity)Activator.CreateInstance(resource.Value);
                    item.Deserialize(node);
                    Entities.Add(item);
                }
            }
            
        }

        private static void LoadAliases()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Data", "aliases.xml");

            if (!File.Exists(path)) return;

            var doc = new XmlDocument();
            doc.Load(path);

            var aliasNodes = doc.SelectNodes("Root/Alias");
            if (aliasNodes == null) return;

            foreach (XmlNode node in aliasNodes)
            {
                // If no attributes - continue
                if (node.Attributes == null) continue;

                // Get the attributes needed
                var entAttr = node.Attributes["Entity"];
                var queryAttr = node.Attributes["Query"];

                // If needed attributes not found - continue
                if (entAttr == null || queryAttr == null)
                    continue;

                // Get entity in question
                var ent = 
                    Entities.FirstOrDefault(
                        o => o.Name.Equals(entAttr.InnerText, StringComparison.InvariantCultureIgnoreCase));

                // Not found - continue
                if (ent == null) continue;

                // Add alias
                ent.Aliases = ent.Aliases == null
                    ? new[] {queryAttr.InnerText}
                    : ent.Aliases.Union(new[] {queryAttr.InnerText}).ToArray();
            }
        }

        public static void LoadData()
        {
            LoadEntities();   
            LoadAliases();
        }

        /// <summary>
        /// Search entities' properties with the given query
        /// </summary>
        /// <param name="query">Single query</param>
        public static IEnumerable<SearchResult> PerformSearchQuery(string query)
        {
            var output = new List<SearchResult>();

            foreach (var ent in Entities)
            {
                List<string> properties;
                if (ent.ContainsInProperties(query, out properties))
                    output.Add(new SearchResult(ent, properties));
            }

            return output;
        }

        /// <summary>
        /// Search entities' properties with the given queries
        /// </summary>
        /// <param name="queries">Multiple queries</param>
        public static IEnumerable<SearchResult> PerformSearchQuery(IEnumerable<string> queries)
        {
            IEnumerable<SearchResult> output = null;

            foreach (string query in queries)
            {
                // First query ----v
                if (output == null)
                {
                    output = PerformSearchQuery(query);
                    continue;
                }

                // Other queries --v
                var newResults = PerformSearchQuery(query);
                output = SearchResult.Intersect(output, newResults);
            }

            return output;
        }
    }
}