using System.Reflection;
using BepInEx;
using RoR2;
using UnityEngine;
using R2API;
using R2API.Utils;

using System;
using System.IO;
using System.Linq;
using MonoMod.Cil;

namespace CurbYourDeath
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Arbition.CurbYourDeath", "Curb Your Death", "1.2.0")]
    [R2APISubmoduleDependency(nameof(SoundAPI))]
    public class CurbYourDeath : BaseUnityPlugin
    {

        private uint eventId;
        public static bool curbPlaying;

        public void Awake()
        {
            using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CurbYourDeath.CYE.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundAPI.SoundBanks.Add(bytes);
            }

            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += (orig, self, damageReport, victimNetworkUser) =>
            {
                int extraLives = damageReport.victimBody.inventory.GetItemCount(RoR2Content.Items.ExtraLife);
                if (extraLives == 0)
                {
                    eventId = AkSoundEngine.PostEvent(2106046636, base.gameObject);
                    curbPlaying = true;
                }
                orig(self, damageReport, victimNetworkUser);
            };

            On.RoR2.SceneObjectToggleGroup.OnServerSceneChanged += (orig, self) =>
            {
                AkSoundEngine.StopPlayingID(eventId, 5);
                curbPlaying = false;
                orig(self);
            };
        }
    }
}