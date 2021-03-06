﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Heroes.Icons.Xml
{
    internal class MapBackgroundsXml : XmlBase, IMapBackgrounds
    {
        private readonly string IconFolderName = "MapBackgrounds";

        private Dictionary<string, string> MapStringByMapRealName = new Dictionary<string, string>();
        private Dictionary<string, Color> MapFontGlowColorByMapRealName = new Dictionary<string, Color>();
        private Dictionary<string, string> MapRealNameByMapAlternativeName = new Dictionary<string, string>();
        private Dictionary<string, string> MapRealNameByMapAliasName = new Dictionary<string, string>();
        private List<string> CustomOnlyMaps = new List<string>();

        private MapBackgroundsXml(string parentFile, string xmlBaseFolder, int currentBuild, bool logger)
            : base(currentBuild, logger)
        {
            XmlParentFile = parentFile;
            XmlBaseFolder = xmlBaseFolder;
            XmlFolder = xmlBaseFolder;
        }

        public static MapBackgroundsXml Initialize(string parentFile, string xmlBaseFolder, int currentBuild, bool logger)
        {
            MapBackgroundsXml xml = new MapBackgroundsXml(parentFile, xmlBaseFolder, currentBuild, logger);
            xml.Parse();
            return xml;
        }

        public Stream GetMapBackground(string mapRealName)
        {
            try
            {
                if (MapStringByMapRealName.ContainsKey(mapRealName))
                {
                    return HeroesIcons.GetHeroesIconsAssembly().GetManifestResourceStream(MapStringByMapRealName[mapRealName]);
                }
                else
                {
                    LogReferenceNameNotFound($"Map background: {mapRealName}");
                    return null;
                }
            }
            catch (IOException)
            {
                LogReferenceNameNotFound($"Map background: {mapRealName}");
                return null;
            }
        }

        public Color GetMapBackgroundFontGlowColor(string mapRealName)
        {
            if (MapFontGlowColorByMapRealName.TryGetValue(mapRealName, out Color color))
                return color;
            else
                return Color.Black;
        }

        /// <summary>
        /// Returns a list of all maps (real names)
        /// </summary>
        /// <returns></returns>
        public List<string> GetMapsList()
        {
            return new List<string>(MapStringByMapRealName.Keys);
        }

        /// <summary>
        /// Returns a list of all maps, except custom only maps
        /// </summary>
        /// <returns></returns>
        public List<string> GetMapsListExceptCustomOnly()
        {
            var allMaps = new Dictionary<string, string>(MapStringByMapRealName);
            foreach (var customMap in GetCustomOnlyMapsList())
            {
                if (allMaps.ContainsKey(customMap))
                {
                    allMaps.Remove(customMap);
                }
            }

            return new List<string>(allMaps.Keys);
        }

        /// <summary>
        /// Returns a list of custom only maps
        /// </summary>
        /// <returns></returns>
        public List<string> GetCustomOnlyMapsList()
        {
            return CustomOnlyMaps;
        }

        /// <summary>
        /// Returns the map name from the map alternative name
        /// </summary>
        /// <param name="mapAlternativeName">map's alternative name</param>
        /// <returns></returns>
        public string GetMapNameByMapAlternativeName(string mapAlternativeName)
        {
            // no pick
            if (string.IsNullOrEmpty(mapAlternativeName))
                return string.Empty;

            if (MapRealNameByMapAlternativeName.TryGetValue(mapAlternativeName, out string mapName))
                return mapName;
            else
                return null;
        }

        /// <summary>
        /// Gets the english name of the given alias. Returns true if found, otherwise false
        /// </summary>
        /// <param name="mapNameAlias">Alias name</param>
        /// <param name="mapNameEnglish">English name</param>
        /// <returns></returns>
        public bool MapNameTranslation(string mapNameAlias, out string mapNameEnglish)
        {
            if (string.IsNullOrEmpty(mapNameAlias))
            {
                mapNameEnglish = string.Empty;
                return false;
            }

            return MapRealNameByMapAliasName.TryGetValue(mapNameAlias.Replace(",", string.Empty), out mapNameEnglish);
        }

        public int TotalCountOfMaps()
        {
            return XmlChildFiles.Count;
        }

        protected override void ParseChildFiles()
        {
            try
            {
                foreach (var mapBackground in XmlChildFiles)
                {
                    using (XmlReader reader = XmlReader.Create(Path.Combine(XmlMainFolderName, XmlBaseFolder, $"{mapBackground}{DefaultFileExtension}"), GetXmlReaderSettings()))
                    {
                        reader.MoveToContent();

                        if (reader.Name != mapBackground)
                            continue;

                        // get the real map background name
                        // example BattlefieldofEternity -> (real) Battlefield of Eternity
                        string realMapBackgroundName = reader["name"];
                        if (string.IsNullOrEmpty(realMapBackgroundName))
                            realMapBackgroundName = mapBackground; // default

                        string alternativeName = reader["alt"];
                        string custom = reader["custom"] ?? "false";

                        if (!bool.TryParse(custom, out bool isCustomOnly))
                            isCustomOnly = false;

                        if (!string.IsNullOrEmpty(alternativeName))
                            MapRealNameByMapAlternativeName.Add(alternativeName, realMapBackgroundName);

                        reader.Read();
                        if (reader.Name == "Normal")
                        {
                            string fontGlow = reader["fontglow"];

                            if (isCustomOnly)
                                CustomOnlyMaps.Add(realMapBackgroundName);

                            if (reader.Read())
                            {
                                MapStringByMapRealName.Add(realMapBackgroundName, SetImageStreamString(IconFolderName, reader.Value));
                                MapFontGlowColorByMapRealName.Add(realMapBackgroundName, ConvertHexToColor(fontGlow));

                                reader.Read();
                            }
                        }

                        reader.Read();
                        if (reader.Name == "Aliases")
                        {
                            if (reader.Read())
                            {
                                string[] aliases = reader.Value.Split(',');

                                // add the english name
                                MapRealNameByMapAliasName.Add(realMapBackgroundName, realMapBackgroundName);

                                // add all the other aliases
                                foreach (var alias in aliases)
                                {
                                    if (MapRealNameByMapAliasName.ContainsKey(alias))
                                        throw new ArgumentException($"Alias already added to {realMapBackgroundName}: {alias}");

                                    MapRealNameByMapAliasName.Add(alias, realMapBackgroundName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ParseXmlException($"Error on xml parsing on {XmlParentFile}", ex);
            }
        }
    }
}
