using System.Reflection;
using BepInEx;
using R2API.AssetPlus;
using RoR2;
using UnityEngine;

using System;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using AK.Wwise;
using IL.RoR2.Audio;
using EntityStates.Huntress;
using MonoMod.Cil;

namespace CurbYourDeath
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Arbition.CurbYourDeath", "Curb Your Death", "1.0.0")]
    public class CurbYourDeath : BaseUnityPlugin
    {

        public static bool curbPlaying;
        public static void AddSoundBank()
        {
            byte[] array = CurbYourDeath.LoadEmbeddedResource("CurbYourDeath.CYE.bnk");
            if (array != null)
            {
                SoundBanks.Add(array);
                return;
            }
            Debug.LogError("SoundBank Fetching Failed");
        }

        private static byte[] LoadEmbeddedResource(string resourceName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            resourceName = executingAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(resourceName));
            byte[] result;
            using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                Stream stream = manifestResourceStream;
                if (stream == null)
                {
                    throw new InvalidOperationException();
                }
                using (BinaryReader binaryReader = new BinaryReader(stream))
                {
                    result = binaryReader.ReadBytes(Convert.ToInt32(manifestResourceStream.Length.ToString()));
                }
            }
            return result;
        }

        public void Awake()
        {
            Chat.AddMessage("Loaded CurbYourDeath!");
            CurbYourDeath.AddSoundBank();

            uint soundId = 2106046636;
            uint eventId = 0;
            uint bgMusicId = RoR2.WwiseUtils.CommonWwiseIds.gameplay;

            On.RoR2.GameOverController.CallRpcClientGameOver += (orig, self) =>
                {
                    AkSoundEngine.StopPlayingID(bgMusicId);
                    //eventId = AkSoundEngine.PostEvent(2106046636, NetworkUser.readOnlyLocalPlayersList[0].GetCurrentBody().gameObject);
                    eventId = AkSoundEngine.PostEvent(2106046636, base.gameObject);
                    curbPlaying = true;
                    orig(self);
                };
            On.RoR2.SceneObjectToggleGroup.OnServerSceneChanged += (orig, self) =>
                {
                    AkSoundEngine.StopPlayingID(eventId, 5);
                    curbPlaying = false;
                    orig(self);
                };
            IL.RoR2.MusicController.LateUpdate += il =>
            {
                var cursor = new ILCursor(il);

                cursor.GotoNext(i => i.MatchStloc(out _));
                cursor.EmitDelegate<Func<bool, bool>>(b =>
                {
                    if (b)
                        return true;

                    return curbPlaying;
                });
            };
        }


        public void Update()
        {

        }

    }
}