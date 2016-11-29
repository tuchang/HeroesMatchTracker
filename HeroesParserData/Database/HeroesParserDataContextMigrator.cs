﻿using HeroesParserData.Database.Migrations;
using System;
using System.Collections.Generic;

namespace HeroesParserData.Database
{
    public class HeroesParserDataContextMigrator
    {
        public Dictionary<int, List<string>> Migrations { get; set; } = new Dictionary<int, List<string>>();
        public Dictionary<int, List<IMigrationAddon>> MigrationAddons { get; set; } = new Dictionary<int, List<IMigrationAddon>>();

        public HeroesParserDataContextMigrator()
        {
            // add call to MigrationVersionX() here
            MigrationVersion1();
            MigrationVersion2();
            MigrationVersion3();
        }

        // Add new migration versions here
        // Each one will be a new method, MigrationVersion1, MigrationVersion2, MigrationVersion3, etc....

        private void MigrationVersion1()
        {
            List<string> steps = new List<string>();
            steps.Add(@"CREATE TABLE IF NOT EXISTS ReplayRenamedPlayers(
                        RenamedPlayerId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                        PlayerId INTEGER,
                        BattleTagName NVARCHAR (50),
                        BattleNetId INTEGER,
                        BattleNetRegionId INTEGER,
                        BattleNetSubId INTEGER,
                        DateAdded DATETIME,
                        FOREIGN KEY (PlayerId) REFERENCES ReplayAllHotsPlayers (PlayerId))");

            Migrations.Add(1, steps);

            List<IMigrationAddon> addonSteps = new List<IMigrationAddon>();
            addonSteps.Add(new MigrationAddon1_2_0_1());
            addonSteps.Add(new MigrationAddon1_2_0_2());

            MigrationAddons.Add(1, addonSteps);
        }

        private void MigrationVersion2()
        {
            List<string> steps = new List<string>();
            steps.Add(@"CREATE TABLE IF NOT EXISTS UserSettings(
                        SettingId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                        Name NVARCHAR,
                        Value NVARCHAR)");

            Migrations.Add(2, steps);

            List<IMigrationAddon> addonSteps = new List<IMigrationAddon>();
            addonSteps.Add(new MigrationAddon1_3_0_1());
            addonSteps.Add(new MigrationAddon1_3_0_2());
            addonSteps.Add(new MigrationAddon1_3_0_3());

            MigrationAddons.Add(2, addonSteps);
        }

        private void MigrationVersion3()
        {
            string text = string.Empty;

            List<string> steps = new List<string>();
            steps.Add(@"CREATE TABLE IF NOT EXISTS ReleaseNotes(
                        Version NVARCHAR PRIMARY KEY NOT NULL,
                        PreRelease BOOLEAN,
                        DateReleased DateTime,
                        PatchNote TEXT)");

            steps.Add(@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote) 
                        VALUES ('0.9.0', 1, '2016-08-12T03:55:53Z',
                        '- Initial release')");

            text = "- Changed the database type to SQLite\r\n- Added an icon reference for Kaelthas\r\n\r\nNote: Auriel''s icon references are still not updated";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote) 
                        VALUES ('0.10.0', 1, '2016-08-14T00:29:51Z',
                        '{text}')");

            text = "- Added Observers, Hero Bans, Score Summary to match lists\r\n- Redesigned Talent table\r\n- Last Game tab changed to latest 30 games\r\n- Added more icon references\r\n- Added Auriel''s icons\r\n- Added logging of missing icons or reference''s to icons\r\n- Redesigned Replays tab\r\n- SQLite query fixes\r\n- Moved all logs text files to log folder\r\n- Database changes - **be sure to delete the old database file**";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)  
                        VALUES ('0.11.0', 1, '2016-08-22T01:24:08Z',
                        '{text}')");

            text = "- Added new option in Replays Tab\r\n- Added some properties to the user settings file";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)   
                        VALUES ('0.11.1', 1, '2016-08-22T22:48:37Z',
                        '{text}')");

            text = "- Added chat to match summary\r\n- Added check to see if replay file exists before parsing\r\n- Fixed replays files not automatically parsing if Watch is selected";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)   
                        VALUES ('0.12.0', 1, '2016-08-23T23:51:36Z',
                        '{text}')");

            text = "- Added application icon\r\n- Added Last Match tab\r\n- Added map backgrounds to Games Modes tabs\r\n- Added Heroes Usable\r\n- Added Home tab\r\n- Added some settings\r\n- Talents and Score Summary now show if a player is silenced\r\n- Replay parsing is now faster due to an update from Heroes.ReplayParser\r\n- Removed Fast Parsed from Replays tab\r\n- Updated for Machines of War and Season 2\r\n- Bug fixes\r\n- **Delete any prior database before starting application**";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)  
                        VALUES ('0.13.0', 1, '2016-09-14T17:53:35Z',
                        '{text}')");

            text = "- Updated replay parser\r\n- Fix issue with the manual replay selector option";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)
                        VALUES ('0.13.1', 1, '2016-09-15T00:23:36Z',
                        '{text}')");

            text = "- Added option to Replay tab to include subdirectories\r\n- Added setting to show / hide battletags\r\n- Finished added MVP award names\r\n- Fixed location of silence icon\r\n- Added total count for season games\r\n- Updated Heroes.ReplayParser";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote) 
                        VALUES ('0.14.0', 1, '2016-09-16T18:59:43Z',
                        '{text}')");

            text = "- Official release\r\n- Updated packages";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)
                        VALUES ('1.0.0', 0, '2016-09-17T18:34:52Z',
                        '{text}')");

            text = "- Updated for Zarya / Warhead Junction patch\r\n- Added Statistics tab\r\n- Only has Stats overview tab, more will come later\r\n- Added date / time and replay length to match titles\r\n- Fixed `Last Match` tab to actually show the last parsed replay instead of latest";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote) 
                        VALUES ('1.1.0', 0, '2016-09-28T01:17:24Z',
                        '{text}')");

            text = "- Updated for Samuro patch\r\n- Added `Statistics - Heroes`\r\n- Added missing Warhead Junction map on Statistics - Overview\r\n- Added startup window\r\n- Will perform updates, database migrations\r\n- Added Season option for match listings\r\n- Added party icon indicators";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)  
                        VALUES ('1.2.0', 0, '2016-10-19T03:42:24Z',
                        '{text}')");

            text = "- Fixed Hero League and Team League award columns in `Statistics - Heroes` tab to correctly show the correct count\r\n- Fixed Azmodan icon reference change";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)  
                        VALUES ('1.2.1', 0, '2016-10-21T21:46:15Z',
                        '{text}')");

            text = "## Fixes\r\n\r\n- Updated Heroes.ReplayPaser\r\n- Non-arena Brawls will now be parsed\r\n  - A dedicated Game Mode tab for Brawls will be available in v1.3.0\r\n- Replays after November 2, 2016 7pm CDT will now be parsed\r\n- Fixed version string in the update log\r\n\r\n**Note: User settings will not be saved when updating, a fix will be available in v1.3.0**";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote)  
                        VALUES ('1.2.2', 0, '2016-11-03T18:17:15Z',
                        '{text}')");

            text = "## What''s New\r\n- Updated for Varian patch\r\n- Talents picks/winrates to `Statistics - Heroes`\r\n- Total column to the `Statistics - Heroes Awards` tab\r\n- Map dropdown filtering for `Statistics - Heroes Awards` tab\r\n- Map and game mode dropdowns filtering for `Statistics - Heroes` talents tab\r\n- Score totals to the `Match Score Summaries`\r\n- Talent descriptions\r\n  - Hover over the talent icon to show the short description\r\n  - Click on the talent icon to show the full description\r\n  - Descriptions shown will be based on the replay build, unless viewing the description in the `Statistics` tab which will always show the latest\r\n- Brawl game tab\r\n - Will only show non-arena brawls (that is, only brawls that save a replay)\r\n- `Replays` tab will now show the build number when the replay gets parsed (successful or not)\r\n- Added map backgrounds to the match tabs when no replay is selected\r\n\r\n## Fixes\r\n- User settings are now saved in the database and will be saved when updating to a new version\r\n  - **When updating to this version, your settings from any previous version will not be saved**\r\n- Fixed duplicate values to appear in the `Statistics` tab tables when switching quickly between tabs\r\n- Fixed a logic error when setting your battletag for the `Statistics` tab\r\n- Observers for `Custom Games` will now show the correct player for any future parsed replays\r\n  - **Any previous parsed replays will not be corrected**\r\n- Last Match, 30 Latest Match, Games Modes will now show the correct party icons (it wasn''t showing all parties before)\r\n\r\n## Other\r\n- MatchAwards, MapBackgrounds, HomeScreens will now be loaded via xml\r\n- Only one instance of the application can run\r\n- The version string will now show the correct build version in the update log\r\n- Hero images in the `Player Heroes tab and Statistics - Heroes` have been changed";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote) 
                        VALUES ('1.3.0', 0, '2016-11-16T04:53:08Z',
                        '{text}')");

            text = "## Fixes\r\n- Match summary tables are no longer editable\r\n- Updated `Nova''s` level 1 talent references for build 48027\r\n- Removed `Greymane''s` level 1 talent `Scented Tincture` from build 48027";
            text = text.Replace("\r\n", Environment.NewLine);
            steps.Add($@"INSERT INTO ReleaseNotes(Version, PreRelease, DateReleased, PatchNote) 
                        VALUES ('1.3.1', 0, '2016-11-20T00:47:35Z',
                        '{text}')");

            steps.Add(@"INSERT INTO UserSettings(Name, Value)
                        VALUES ('IsNewUpdateApplied', 'True')");

            Migrations.Add(3, steps);
        }
    }
}
