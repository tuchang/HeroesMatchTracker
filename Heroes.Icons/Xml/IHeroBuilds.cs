﻿using Heroes.Icons.Models;
using System;
using System.Collections.Generic;

namespace Heroes.Icons.Xml
{
    public interface IHeroBuilds
    {
        /// <summary>
        /// Get the patch notes link from the given build number. Returns null if not found
        /// </summary>
        /// <param name="build">The build number</param>
        /// <returns></returns>
        Tuple<string, string> GetPatchNotes(int build);

        /// <summary>
        /// Returns a Hero object
        /// </summary>
        /// <param name="heroName">Can be the real hero name or short name</param>
        /// <returns></returns>
        Hero GetHeroInfo(string heroName);

        /// <summary>
        /// Returns the real hero name from the hero's attribute id
        /// </summary>
        /// <param name="attributeId">Four character hero id</param>
        /// <returns>Full hero name</returns>
        string GetRealHeroNameFromAttributeId(string attributeId);

        /// <summary>
        /// Returns the real hero name from the short name
        /// </summary>
        /// <param name="shortName">short name of hero</param>
        /// <returns></returns>
        string GetRealHeroNameFromShortName(string shortName);

        /// <summary>
        /// Returns the real hero name from the hero unit name
        /// </summary>
        /// <param name="heroUnitName">hero unit name</param>
        /// <returns></returns>
        string GetRealHeroNameFromHeroUnitName(string heroUnitName);

        /// <summary>
        /// Checks to see if the hero name exists
        /// </summary>
        /// <param name="heroName">Real name of hero or alt name</param>
        /// <returns>True if found</returns>
        bool HeroExists(string heroName);

        /// <summary>
        /// Returns a list of (real) hero names for the given build
        /// </summary>
        /// <param name="build">The build number</param>
        /// <returns></returns>
        List<string> GetListOfHeroes(int build);

        /// <summary>
        /// Returns the total amount of heroes (latest build)
        /// </summary>
        /// <returns></returns>
        int TotalAmountOfHeroes();
    }
}
