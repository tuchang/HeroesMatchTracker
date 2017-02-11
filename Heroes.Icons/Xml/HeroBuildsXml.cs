﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Heroes.Icons.Xml
{
    internal class HeroBuildsXml : XmlBase, IHeroBuilds
    {
        private const string ShortTalentTooltipFileName = "_ShortTalentTooltips.txt";
        private const string FullTalentTooltipFileName = "_FullTalentTooltips.txt";

        private int SelectedBuild;
        private HeroesXml HeroesXml;
        private bool Logger;

        private Dictionary<string, string> TalentShortTooltip = new Dictionary<string, string>();
        private Dictionary<string, string> TalentLongTooltip = new Dictionary<string, string>();

        /// <summary>
        /// key is reference name of talent
        /// Tuple: key is real name of talent
        /// </summary>
        private Dictionary<string, Tuple<string, Uri>> RealTalentNameUriByReferenceName = new Dictionary<string, Tuple<string, Uri>>();

        /// <summary>
        /// key is real hero name
        /// value is a string of all talent reference names for that tier
        /// </summary>
        private Dictionary<string, Dictionary<TalentTier, List<string>>> HeroTalentsListByRealName = new Dictionary<string, Dictionary<TalentTier, List<string>>>();

        /// <summary>
        /// key is the talent reference name
        /// </summary>
        private Dictionary<string, TalentTooltip> HeroTalentTooltipsByRealName = new Dictionary<string, TalentTooltip>();

        /// <summary>
        /// key is the talent reference name
        /// </summary>
        private Dictionary<string, string> RealHeroNameByTalentReferenceName = new Dictionary<string, string>();

        private HeroBuildsXml(string parentFile, string xmlBaseFolder, HeroesXml heroesXml, bool logger, int? build = null)
            : base(build.HasValue ? build.Value : 0)
        {
            Logger = logger;
            XmlParentFile = parentFile;
            XmlBaseFolder = xmlBaseFolder;
            HeroesXml = heroesXml;

            if (build == null)
                SetDefaultBuildDirectory();
            else
                SelectedBuild = build.Value;

            XmlFolder = Path.Combine(xmlBaseFolder, SelectedBuild.ToString());
        }

        public int CurrentLoadedHeroesBuild { get { return SelectedBuild; } }
        public int EarliestHeroesBuild { get; private set; } // cleared once initialized
        public int LatestHeroesBuild { get; private set; } // cleared once initialized
        public List<int> Builds { get; private set; } = new List<int>();

        public static HeroBuildsXml Initialize(string parentFile, string xmlBaseFolder, HeroesXml heroesXml, bool logger, int? build = null)
        {
            if (heroesXml == null)
                return null;

            HeroBuildsXml xml = new HeroBuildsXml(parentFile, xmlBaseFolder, heroesXml, logger, build);
            xml.Parse();
            return xml;
        }

        /// <summary>
        /// Returns a BitmapImage of the talent
        /// </summary>
        /// <param name="talentReferenceName">Reference talent name</param>
        /// <returns>BitmapImage of the talent</returns>
        public BitmapImage GetTalentIcon(string talentReferenceName)
        {
            Tuple<string, Uri> talent;

            // no pick
            if (string.IsNullOrEmpty(talentReferenceName))
                return HeroesBitmapImage(@"Talents\_Generic\storm_ui_icon_no_pick.dds");

            if (RealTalentNameUriByReferenceName.TryGetValue(talentReferenceName, out talent))
            {
                BitmapImage image = new BitmapImage(talent.Item2);
                image.Freeze();

                return image;
            }
            else
            {
                if (Logger)
                    LogReferenceNameNotFound($"Talent icon: {talentReferenceName}");

                return HeroesBitmapImage(@"Talents\_Generic\storm_ui_icon_default.dds");
            }
        }

        /// <summary>
        /// Returns the talent name from the talent reference name
        /// </summary>
        /// <param name="talentReferenceName">Reference talent name</param>
        /// <returns>Talent name</returns>
        public string GetTrueTalentName(string talentReferenceName)
        {
            Tuple<string, Uri> talent;

            // no pick
            if (string.IsNullOrEmpty(talentReferenceName))
                return "No pick";

            if (RealTalentNameUriByReferenceName.TryGetValue(talentReferenceName, out talent))
            {
                return talent.Item1;
            }
            else
            {
                if (Logger)
                    LogReferenceNameNotFound($"No name for reference: {talentReferenceName}");

                return talentReferenceName;
            }
        }

        /// <summary>
        /// Returns a dictionary of all the talents of a hero
        /// </summary>
        /// <param name="realHeroName">real hero name</param>
        /// <returns></returns>
        public Dictionary<TalentTier, List<string>> GetTalentsForHero(string realHeroName)
        {
            Dictionary<TalentTier, List<string>> talents;
            if (HeroTalentsListByRealName.TryGetValue(realHeroName, out talents))
            {
                return talents;
            }
            else
            {
                if (Logger)
                    LogReferenceNameNotFound($"No hero real name found [{nameof(GetTalentsForHero)}]: {realHeroName}");

                return null;
            }
        }

        /// <summary>
        /// Returns a TalentTooltip object which contains the short and full tooltips of the talent
        /// </summary>
        /// <param name="talentReferenceName">Talent reference name</param>
        /// <returns></returns>
        public TalentTooltip GetTalentTooltips(string talentReferenceName)
        {
            TalentTooltip talentTooltip = new TalentTooltip(string.Empty, string.Empty);

            if (string.IsNullOrEmpty(talentReferenceName) || !HeroTalentTooltipsByRealName.ContainsKey(talentReferenceName))
                return talentTooltip;

            HeroTalentTooltipsByRealName.TryGetValue(talentReferenceName, out talentTooltip);

            return talentTooltip;
        }

        /// <summary>
        /// Gets the hero name associated with the given talent. Returns true is found, otherwise returns false
        /// </summary>
        /// <param name="talentName">The talent reference name</param>
        /// <param name="heroRealName">The hero name</param>
        /// <returns></returns>
        public bool GetHeroNameFromTalentReferenceName(string talentName, out string heroRealName)
        {
            return RealHeroNameByTalentReferenceName.TryGetValue(talentName, out heroRealName);
        }

        public List<int> GetListOfHeroesBuilds()
        {
            return Builds;
        }

        protected override void Parse()
        {
            LoadTalentTooltipStrings();
            base.Parse();
        }

        protected override void ParseChildFiles()
        {
            try
            {
                foreach (var hero in XmlChildFiles)
                {
                    using (XmlReader reader = XmlReader.Create($@"Xml\{XmlBaseFolder}\{SelectedBuild}\{hero}.xml"))
                    {
                        reader.MoveToContent();

                        string heroAltName = reader.Name;
                        if (heroAltName != hero)
                            continue;

                        var talentTiersForHero = new Dictionary<TalentTier, List<string>>();

                        // add talents, read each tier
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                var talentTierList = new List<string>();
                                TalentTier tier;

                                // is tier Level1, Level4, etc...
                                if (Enum.TryParse(reader.Name, out tier))
                                {
                                    // read each talent in tier
                                    while (reader.Read() && reader.Name != tier.ToString())
                                    {
                                        if (reader.NodeType == XmlNodeType.Element)
                                        {
                                            string refName = reader.Name; // reference name of talent
                                            string realName = reader["name"] == null ? string.Empty : reader["name"];  // real ingame name of talent
                                            string generic = reader["generic"] == null ? "false" : reader["generic"];  // is the icon being used generic
                                            string desc = reader["desc"] == null ? string.Empty : reader["desc"]; // reference name for talent desciptions

                                            SetTalentTooltip(refName, desc);

                                            bool isGeneric;
                                            if (!bool.TryParse(generic, out isGeneric))
                                                isGeneric = false;

                                            if (reader.Read())
                                            {
                                                if (refName.StartsWith("Generic") || refName.StartsWith("HeroGeneric") || refName.StartsWith("BattleMomentum"))
                                                    isGeneric = true;

                                                if (!RealTalentNameUriByReferenceName.ContainsKey(refName))
                                                    RealTalentNameUriByReferenceName.Add(refName, new Tuple<string, Uri>(realName, SetHeroTalentUri(hero, reader.Value, isGeneric)));

                                                talentTierList.Add(refName);

                                                if (!isGeneric)
                                                {
                                                    if (!HeroesXml.HeroExists(heroAltName, false))
                                                        throw new ArgumentException($"Hero alt name not found: {heroAltName}");

                                                    if (RealHeroNameByTalentReferenceName.ContainsKey(refName) && tier != TalentTier.Old)
                                                        throw new ArgumentException($"Same key {refName}");

                                                    if (tier != TalentTier.Old)
                                                        RealHeroNameByTalentReferenceName.Add(refName, HeroesXml.GetRealHeroNameFromAltName(heroAltName));
                                                }
                                            }
                                        }
                                    }

                                    talentTiersForHero.Add(tier, talentTierList);
                                }
                            }
                        } // end while

                        if (!HeroesXml.HeroExists(heroAltName, false))
                            throw new ArgumentException($"Hero alt name not found: {heroAltName}");

                        HeroTalentsListByRealName.Add(HeroesXml.GetRealHeroNameFromAltName(heroAltName), talentTiersForHero);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ParseXmlException("Error on parsing of hero xml files", ex);
            }
        }

        // this should only run once on startup
        private void SetDefaultBuildDirectory()
        {
            List<string> buildDirectories = Directory.GetDirectories($@"Xml\{XmlBaseFolder}").ToList();

            foreach (var directory in buildDirectories)
            {
                int buildNumber;
                if (int.TryParse(Path.GetFileName(directory), out buildNumber))
                    Builds.Add(buildNumber);
            }

            if (buildDirectories.Count < 1 || Builds.Count < 1)
                throw new ParseXmlException("No HeroBuilds folders found!");

            Builds = Builds.OrderByDescending(x => x).ToList();

            EarliestHeroesBuild = Builds[Builds.Count - 1];
            LatestHeroesBuild = SelectedBuild = Builds[0];
        }

        private Uri SetHeroPortraitUri(string fileName)
        {
            return new Uri($@"{ApplicationIconsPath}\HeroPortraits\{fileName}", UriKind.Absolute);
        }

        private Uri SetLoadingPortraitUri(string fileName)
        {
            return new Uri($@"{ApplicationIconsPath}\HeroLoadingScreenPortraits\{fileName}", UriKind.Absolute);
        }

        private Uri SetLeaderboardPortraitUri(string fileName)
        {
            return new Uri($@"{ApplicationIconsPath}\HeroLeaderboardPortraits\{fileName}", UriKind.Absolute);
        }

        private Uri SetHeroTalentUri(string hero, string fileName, bool isGenericTalent)
        {
            if (!(Path.GetExtension(fileName) != ".dds" || Path.GetExtension(fileName) != ".png"))
                throw new HeroesIconException($"Image file does not have .dds or .png extension [{fileName}]");

            if (!isGenericTalent)
                return new Uri($@"{ApplicationIconsPath}\Talents\{hero}\{fileName}", UriKind.Absolute);
            else
                return new Uri($@"{ApplicationIconsPath}\Talents\_Generic\{fileName}", UriKind.Absolute);
        }

        private void SetTalentTooltip(string talentReferenceName, string desc)
        {
            // checking keys because of generic talents
            if (!HeroTalentTooltipsByRealName.ContainsKey(talentReferenceName) && !string.IsNullOrEmpty(desc))
            {
                string shortDesc;
                string longDesc;
                if (!TalentShortTooltip.TryGetValue(desc, out shortDesc))
                    shortDesc = string.Empty;

                if (!TalentLongTooltip.TryGetValue(desc, out longDesc))
                    longDesc = string.Empty;

                HeroTalentTooltipsByRealName.Add(talentReferenceName, new TalentTooltip(shortDesc, longDesc));
            }
        }

        private void LoadTalentTooltipStrings()
        {
            try
            {
                using (StreamReader reader = new StreamReader($@"Xml\{XmlBaseFolder}\{SelectedBuild}\{ShortTalentTooltipFileName}"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!line.StartsWith("--"))
                        {
                            string[] talent = line.Split(new char[] { '=' }, 2);
                            TalentShortTooltip.Add(talent[0], talent[1]);
                        }
                    }
                }

                using (StreamReader reader = new StreamReader($@"Xml\{XmlBaseFolder}\{SelectedBuild}\{FullTalentTooltipFileName}"))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!line.StartsWith("--"))
                        {
                            string[] talent = line.Split(new char[] { '=' }, 2);
                            TalentLongTooltip.Add(talent[0], talent[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ParseXmlException("Error on loading talent tooltips", ex);
            }
        }
    }
}