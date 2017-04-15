﻿using Heroes.Helpers;
using Heroes.Icons;
using Heroes.ReplayParser;
using HeroesMatchData.Data.Databases;
using HeroesMatchData.Data.Models.Replays;
using HeroesMatchData.Data.Queries.Settings;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeroesMatchData.Data.Queries.Replays
{
    public class Statistics
    {
        private UserSettings UserSettings = new UserSettings();

        /// <summary>
        /// Gets the total count of wins or losses for given hero on a particular map
        /// </summary>
        /// <param name="character">Hero name</param>
        /// <param name="season">Selected season</param>
        /// <param name="gameMode">Selected GameMode</param>
        /// <param name="isWins">Return wins if true otherwise return losses</param>
        /// <param name="mapName">Selected map</param>
        /// <returns></returns>
        public int ReadTotalGameResults(string character, Season season, GameMode gameMode, bool isWins, string mapName = null)
        {
            var replayBuild = HeroesHelpers.Builds.GetReplayBuildsFromSeason(season);
            int total = 0;

            using (var db = new ReplaysContext())
            {
                foreach (Enum value in Enum.GetValues(gameMode.GetType()))
                {
                    if (gameMode.HasFlag(value))
                    {
                        var query = from mp in db.ReplayMatchPlayers
                                    join r in db.Replays on mp.ReplayId equals r.ReplayId
                                    where mp.PlayerId == UserSettings.UserPlayerId &&
                                          mp.Character == character &&
                                          mp.IsWinner == isWins &&
                                          r.GameMode == (GameMode)value &&
                                          r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2 &&
                                          r.MapName == mapName
                                    select mp.IsWinner;

                        total += query.Count();
                    }
                }

                return total;
            }
        }

        /// <summary>
        /// Gets the score results for a hero on a particular map
        /// </summary>
        /// <param name="character">Hero name</param>
        /// <param name="season">Selected season</param>
        /// <param name="gameMode">Selected GameMode</param>
        /// <param name="mapName">Selected map</param>
        /// <returns></returns>
        public List<ReplayMatchPlayerScoreResult> ReadScoreResult(string character, Season season, GameMode gameMode, string mapName)
        {
            var replayBuild = HeroesHelpers.Builds.GetReplayBuildsFromSeason(season);
            List<ReplayMatchPlayerScoreResult> list = new List<ReplayMatchPlayerScoreResult>();

            using (var db = new ReplaysContext())
            {
                foreach (Enum value in Enum.GetValues(gameMode.GetType()))
                {
                    if ((GameMode)value != GameMode.Unknown && gameMode.HasFlag(value))
                    {
                        var query = from r in db.Replays
                                    join mp in db.ReplayMatchPlayers on r.ReplayId equals mp.ReplayId
                                    join mpsr in db.ReplayMatchPlayerScoreResults on new { mp.ReplayId, mp.PlayerId } equals new { mpsr.ReplayId, mpsr.PlayerId }
                                    where mp.PlayerId == UserSettings.UserPlayerId &&
                                          mp.Character == character &&
                                          r.GameMode == (GameMode)value &&
                                          r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2 &&
                                          r.MapName == mapName
                                    select mpsr;

                        list.AddRange(query.ToList());
                    }
                }

                return list;
            }
        }

        public TimeSpan ReadTotalMapGameTime(string character, Season season, GameMode gameMode, string mapName)
        {
            var replayBuild = HeroesHelpers.Builds.GetReplayBuildsFromSeason(season);

            using (var db = new ReplaysContext())
            {
                var gameModeFilter = PredicateBuilder.New<ReplayMatch>();
                foreach (Enum value in Enum.GetValues(gameMode.GetType()))
                {
                    if ((GameMode)value != GameMode.Unknown && gameMode.HasFlag(value))
                    {
                        Enum temp = value;
                        gameModeFilter = gameModeFilter.Or(x => x.GameMode == (GameMode)temp);
                    }
                }

                var query = from mp in db.ReplayMatchPlayers
                            join r in db.Replays on mp.ReplayId equals r.ReplayId
                            where mp.PlayerId == UserSettings.UserPlayerId &&
                                    mp.Character == character &&
                                    r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2 &&
                                    r.MapName == mapName
                            select r;

                query = query.AsExpandable().Where(gameModeFilter);

                return TimeSpan.FromTicks(query.Count() > 0 ? query.Sum(x => x.ReplayLengthTicks) : 0);
            }
        }

        /// <summary>
        /// Gets the win or loss count of the talent for a given hero
        /// </summary>
        /// <param name="character">Hero name</param>
        /// <param name="season">Selected season</param>
        /// <param name="gameMode">Selected GameMode</param>
        /// <param name="mapName">Selected map</param>
        /// <param name="talentReferenceName">Selected talent reference name</param>
        /// <param name="tier">The tier that the talent is on</param>
        /// <param name="isWinner">Get wins if true otherwise losses</param>
        /// <returns></returns>
        public int ReadTalentsCountForHero(string character, Season season, GameMode gameMode, List<string> maps, string talentReferenceName, TalentTier tier, bool isWinner)
        {
            var replayBuild = HeroesHelpers.Builds.GetReplayBuildsFromSeason(season);
            string talentNameColumn = string.Empty;

            using (var db = new ReplaysContext())
            {
                var gameModeFilter = PredicateBuilder.New<ReplayMatch>();
                foreach (Enum value in Enum.GetValues(gameMode.GetType()))
                {
                    if ((GameMode)value != GameMode.Unknown && gameMode.HasFlag(value))
                    {
                        Enum temp = value;
                        gameModeFilter = gameModeFilter.Or(x => x.GameMode == (GameMode)temp);
                    }
                }

                var mapFilter = PredicateBuilder.New<ReplayMatch>();
                foreach (var map in maps)
                {
                    string temp = map;
                    mapFilter = mapFilter.Or(x => x.MapName == temp);
                }

                IQueryable<ReplayMatch> query = null;

                switch (tier)
                {
                    case TalentTier.Level1:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                    join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                    join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                    where mpt.PlayerId == UserSettings.UserPlayerId &&
                                          mp.IsWinner == isWinner &&
                                          mpt.Character == character &&
                                          mpt.TalentName1 == talentReferenceName &&
                                          r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                    select r;
                        break;
                    case TalentTier.Level4:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                where mpt.PlayerId == UserSettings.UserPlayerId &&
                                      mp.IsWinner == isWinner &&
                                      mpt.Character == character &&
                                      mpt.TalentName4 == talentReferenceName &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                select r;
                        break;
                    case TalentTier.Level7:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                where mpt.PlayerId == UserSettings.UserPlayerId &&
                                      mp.IsWinner == isWinner &&
                                      mpt.Character == character &&
                                      mpt.TalentName7 == talentReferenceName &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                select r;
                        break;
                    case TalentTier.Level10:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                where mpt.PlayerId == UserSettings.UserPlayerId &&
                                      mp.IsWinner == isWinner &&
                                      mpt.Character == character &&
                                      mpt.TalentName10 == talentReferenceName &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                select r;
                        break;
                    case TalentTier.Level13:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                where mpt.PlayerId == UserSettings.UserPlayerId &&
                                      mp.IsWinner == isWinner &&
                                      mpt.Character == character &&
                                      mpt.TalentName13 == talentReferenceName &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                select r;
                        break;
                    case TalentTier.Level16:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                where mpt.PlayerId == UserSettings.UserPlayerId &&
                                      mp.IsWinner == isWinner &&
                                      mpt.Character == character &&
                                      mpt.TalentName16 == talentReferenceName &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                select r;
                        break;
                    case TalentTier.Level20:
                        query = from mpt in db.ReplayMatchPlayerTalents
                                join r in db.Replays on mpt.ReplayId equals r.ReplayId
                                join mp in db.ReplayMatchPlayers on new { mpt.ReplayId, mpt.PlayerId } equals new { mp.ReplayId, mp.PlayerId }
                                where mpt.PlayerId == UserSettings.UserPlayerId &&
                                      mp.IsWinner == isWinner &&
                                      mpt.Character == character &&
                                      mpt.TalentName20 == talentReferenceName &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2
                                select r;
                        break;
                    default:
                        talentNameColumn = null;
                        break;
                }

                query = query.AsExpandable()
                    .Where(gameModeFilter)
                    .Where(mapFilter);

                return query.Count();
            }
        }

        public int ReadMatchAwardCountForHero(string character, Season season, GameMode gameMode, List<string> maps, string mvpAwardType)
        {
            var replayBuild = HeroesHelpers.Builds.GetReplayBuildsFromSeason(season);
            int total = 0;

            using (var db = new ReplaysContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                foreach (var map in maps)
                {
                    var query = from r in db.Replays
                                join mp in db.ReplayMatchPlayers on r.ReplayId equals mp.ReplayId
                                join ma in db.ReplayMatchAwards on new { mp.ReplayId, mp.PlayerId } equals new { ma.ReplayId, ma.PlayerId }
                                where mp.PlayerId == UserSettings.UserPlayerId &&
                                      mp.Character == character &&
                                      r.GameMode == gameMode &&
                                      r.ReplayBuild >= replayBuild.Item1 && r.ReplayBuild < replayBuild.Item2 &&
                                      r.MapName == map &&
                                      ma.Award == mvpAwardType
                                select ma;

                    total += query.Count();
                }

                return total;
            }
        }
    }
}
