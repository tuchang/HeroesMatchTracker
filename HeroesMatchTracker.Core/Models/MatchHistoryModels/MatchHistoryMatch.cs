﻿using GalaSoft.MvvmLight.CommandWpf;
using Heroes.Helpers;
using Heroes.Icons;
using HeroesMatchTracker.Core.Models.MatchModels;
using HeroesMatchTracker.Core.Services;
using HeroesMatchTracker.Core.User;
using HeroesMatchTracker.Core.ViewServices;
using HeroesMatchTracker.Data.Models.Replays;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeroesMatchTracker.Core.Models.MatchHistoryModels
{
    public class MatchHistoryMatch
    {
        private IInternalService InternalService;
        private IHeroesIconsService HeroesIcons;
        private ISelectedUserProfileService UserProfile;
        private IWebsiteService Website;
        private ReplayMatch ReplayMatch;

        public MatchHistoryMatch(IInternalService internalService, IWebsiteService website, ReplayMatch replayMatch)
        {
            InternalService = internalService;
            HeroesIcons = internalService.HeroesIcons;
            UserProfile = internalService.UserProfile;
            Website = website;

            ReplayMatch = replayMatch;

            SetMatch();
        }

        public List<MatchPlayerBase> MatchOverviewTeam1List { get; private set; } = new List<MatchPlayerBase>();
        public List<MatchPlayerBase> MatchOverviewTeam2List { get; private set; } = new List<MatchPlayerBase>();

        public string GameMode { get; private set; }
        public string MapName { get; private set; }
        public Stream UserHeroImage { get; private set; }
        public string UserHero { get; private set; }
        public string WinnerResult { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public TimeSpan GameLength { get; private set; }
        public int? Build { get; private set; }
        public long ReplayId { get; private set; }

        public IMatchSummaryFlyoutService MatchSummaryFlyout => ServiceLocator.Current.GetInstance<IMatchSummaryFlyoutService>();
        public IMatchSummaryReplayService MatchSummaryReplay => ServiceLocator.Current.GetInstance<IMatchSummaryReplayService>();

        public RelayCommand ShowMatchSummaryCommand => new RelayCommand(async () => await ShowMatchSummaryAsync());

        private async Task ShowMatchSummaryAsync()
        {
            if (ReplayMatch == null)
                return;

            await MatchSummaryReplay.LoadMatchSummaryAsync(ReplayMatch, null);

            MatchSummaryFlyout.SetMatchSummaryHeader($"Match Summary [Id:{ReplayMatch.ReplayId}] [Build:{ReplayMatch.ReplayBuild}]");
            MatchSummaryFlyout.OpenMatchSummaryFlyout();
        }

        private void SetMatch()
        {
            var playersList = ReplayMatch.ReplayMatchPlayers.ToList();
            var matchAwardDictionary = ReplayMatch.ReplayMatchAward.ToDictionary(x => x.PlayerId, x => x.Award);

            // load up correct build information
            HeroesIcons.LoadHeroesBuild(ReplayMatch.ReplayBuild);

            var playerParties = PlayerParties.FindPlayerParties(playersList);

            foreach (var player in playersList)
            {
                if (player.Team == 4)
                    continue;

                MatchPlayerBase matchPlayerBase = new MatchPlayerBase(InternalService, Website, player);
                matchPlayerBase.SetPlayerInfo(player.IsAutoSelect, playerParties, matchAwardDictionary);

                // add to collection
                if (player.Team == 0)
                    MatchOverviewTeam1List.Add(matchPlayerBase);
                else if (player.Team == 1)
                    MatchOverviewTeam2List.Add(matchPlayerBase);

                if (player.PlayerId == UserProfile.PlayerId)
                {
                    UserHero = player.Character;
                    UserHeroImage = HeroesIcons.HeroBuilds().GetHeroInfo(player.Character).GetHeroPortrait();
                    WinnerResult = player.IsWinner ? "Win" : "Loss";
                }
            }

            GameMode = ReplayMatch.GameMode.GetFriendlyName();
            MapName = ReplayMatch.MapName;
            TimeStamp = ReplayMatch.TimeStamp.Value;
            GameLength = ReplayMatch.ReplayLength;
            Build = ReplayMatch.ReplayBuild;
            ReplayId = ReplayMatch.ReplayId;
        }
    }
}
