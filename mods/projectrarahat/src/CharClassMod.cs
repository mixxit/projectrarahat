﻿using Foundation.Extensions;
using projectrarahat.src.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace projectrarahat.src
{
    /// <summary>
    /// Controls end of round events
    /// </summary>
    public class CharClassMod : ModSystem
    {
        private ICoreServerAPI api;

        public override void StartPre(ICoreAPI api)
        {
            CharClassModConfigFile.Current = api.LoadOrCreateConfig<CharClassModConfigFile>($"{typeof(CharClassMod).Name}.json");
            api.World.Config.SetBool("loadGearNonDress", CharClassModConfigFile.Current.LoadGearNonDress);
            base.StartPre(api);
        }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            this.api.Event.SaveGameLoaded += new Vintagestory.API.Common.Action(this.OnSaveGameLoaded);
            this.api.Event.PlayerNowPlaying += new PlayerDelegate(this.OnPlayerNowPlaying);
            // Check every 8 seconds
            this.api.World.RegisterGameTickListener(OnGameTick, 8000);
        }

        private void OnPlayerNowPlaying(IServerPlayer player)
        {
            RegisterPlayerClassChangedListener(player);
        }

        private void RegisterPlayerClassChangedListener(IServerPlayer player)
        {
            if (CharClassModState.Instance.GetCharacterClasses() == null || CharClassModState.Instance.GetCharacterClasses().Count < 1)
                return;

            player.Entity.WatchedAttributes.RegisterModifiedListener("characterClass", (System.Action)(() => OnPlayerClassChanged(player)));
        }

        private void OnPlayerClassChanged(IServerPlayer player)
        {
            if (player.IsGrantedInitialItems())
                return;

            player.GrantInitialClassItems();
        }

        private void OnSaveGameLoaded()
        {
            CharClassModState.Instance.SetCharacterClasses(this.api.Assets.Get("config/characterclasses.json").ToObject<List<CharacterClass>>());
            LogUtils<CharClassMod>.LogInfo($"Found {CharClassModState.Instance.GetCharacterClasses().Count()} json character classes");
            LogUtils<CharClassMod>.LogInfo("CharClassMod started");
        }

        private void OnGameTick(float tick)
        {

        }
    }

    public class CharClassModConfigFile
    {
        public static CharClassModConfigFile Current { get; set; }
        public bool LoadGearNonDress { get; set; } = true;
    }

}
