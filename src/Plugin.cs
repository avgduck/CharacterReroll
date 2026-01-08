using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LLBML;
using LLBML.States;
using LLBML.Utils;

namespace CharacterReroll;

[BepInPlugin(GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "avgduck.plugins.llb.characterreroll";
    
    internal static Plugin Instance { get; private set; }
    internal static ManualLogSource LogGlobal { get; private set; }

    private void Awake()
    {
        Instance = this;
        LogGlobal = this.Logger;
        
        Assets.Init();
        Harmony harmony = Harmony.CreateAndPatchAll(typeof(RandomPatch), GUID);
    }

    private static class RandomPatch
    {
        [HarmonyPatch(typeof(PlayersCharacterButton), nameof(PlayersCharacterButton.Init))]
        [HarmonyPostfix]
        private static void PlayersCharacterButton_Init_Postfix(PlayersCharacterButton __instance)
        {
            if (__instance.character is not Character.RANDOM) return;

            __instance.imCharacter.sprite = Assets.SpriteRandom;
            __instance.btCharacter.onClick = (playerNr) =>
            {
                List<Character> unlockedCharacters = CharacterApi.GetPlayableCharacters().Filter(c => ProgressApi.IsUnlocked(c)).ToList();
                GameStates.Send(Msg.SEL_CHAR, playerNr, (int)unlockedCharacters[UnityEngine.Random.RandomRangeInt(0, unlockedCharacters.Count)]);
            };
        }
    }
}