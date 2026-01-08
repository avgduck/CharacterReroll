using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LLBML;
using LLBML.Players;
using LLBML.Settings;
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
        private static bool[] randomSkin = [false, false, false, false];
        
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

                int selector = playerNr;
                if (GameSettings.current.gameMode is GameMode.TRAINING) selector = 0;
                if (GameSettings.IsOnline) selector = Player.LocalPlayerNumber;
                if (selector == -1) return;
                randomSkin[selector] = true;
            };
        }

        // CharacterVariant Player::GetVariantFirst(Character character)
        [HarmonyPatch(typeof(ALDOKEMAOMB), nameof(ALDOKEMAOMB.DIGHIEBNDPE), typeof(Character))]
        [HarmonyPrefix]
        private static bool Player_GetVariantFirst_Prefix(ref CharacterVariant __result, ALDOKEMAOMB __instance, Character DNLBALOLMDF)
        {
            Player player = __instance;
            Character character = DNLBALOLMDF;

            if (!randomSkin[player.nr]) return true;

            player.CharacterSelected = character;
            __result = player.GetVariantRandom();
            randomSkin[player.nr] = false;
            return false;
        }
        
        // CharacterVariant Player::GetVariantNext(int prevNext = 1)
        [HarmonyPatch(typeof(ALDOKEMAOMB), nameof(ALDOKEMAOMB.MKDAGHDAMML))]
        [HarmonyPrefix]
        private static bool Player_GetVariantNext_Prefix(ref CharacterVariant __result, ALDOKEMAOMB __instance)
        {
            Player player = __instance;

            if (!randomSkin[player.nr]) return true;

            CharacterVariant currentSkin = player.variant;
            do
            {
                __result = player.GetVariantRandom();
            } while (__result == currentSkin);
            randomSkin[player.nr] = false;
            return false;
        }
    }
}