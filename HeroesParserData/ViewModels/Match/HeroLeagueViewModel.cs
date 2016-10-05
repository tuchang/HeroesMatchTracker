﻿using Heroes.ReplayParser;
using HeroesParserData.DataQueries;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HeroesParserData.ViewModels.Match
{
    public class HeroLeagueViewModel : MatchContext
    {
        public HeroLeagueViewModel()
            :base()
        {
            HasObservers = false;
            HasBans = true;
        }

        protected override async Task RefreshExecute()
        {
            await QueryMatchList();
        }

        private async Task QueryMatchList()
        {
            MatchList = new ObservableCollection<Models.DbModels.Replay>(await Query.Replay.ReadGameModeRecordsAsync(GameMode.HeroLeague));
            RowsReturned = MatchList.Count;
        }
    }
}
