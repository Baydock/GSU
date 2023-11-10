using GSU.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.Player;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Upgrade;
using Il2CppAssets.Scripts.Utils;
using Il2CppNinjaKiwi.LiNK.AuthenticationProviders;
using Il2CppNinjaKiwi.Players;
using Il2CppSystem.Threading.Tasks;
using MelonLoader;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.U2D;

[assembly: MelonInfo(typeof(GSU.Mod), "GSU", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonColor(255, 148, 148, 148)]
[assembly: MelonAuthorColor(255, 255, 104, 0)]

// TODO: fix upgrade screen
namespace GSU {
    [HarmonyPatch]
    public sealed class Mod : MelonMod {
        public static MelonLogger.Instance Logger { get; private set; }

        public static GameModel GameModel { get; private set; }

        public override void OnInitializeMelon() {
            Logger = LoggerInstance;
        }

        /// <summary>
        /// Adds all tower and upgrade data to the GameModel so that it appears in game
        /// </summary>
        [HarmonyPatch(typeof(GameModelLoader), nameof(GameModelLoader.Load))]
        [HarmonyPostfix]
        public static void Load(ref GameModel __result) {
            GameModel = __result;

            AddTower(GSU.Details, GSU.After,
                new TowerModel[] {
                    GSU.Tower0,
                    GSU.Tower1,
                    GSU.Tower2,
                    GSU.Tower3
                },
                new UpgradeModel[] {
                    GSU.Upgrade1,
                    GSU.Upgrade2,
                    GSU.Upgrade3
                });
            GSU.AddLocalization();
        }

        private static void AddTower(ShopTowerDetailsModel details, string after, TowerModel[] towers, UpgradeModel[] upgrades) {
            int index = 0;
            bool foundIndex = false;
            for (int i = 0; i < GameModel.towerSet.Length; i++) {
                if (foundIndex) {
                    GameModel.towerSet[i].towerIndex++;
                } else if (GameModel.towerSet[i].towerId.Equals(after)) {
                    foundIndex = true;
                    index = i + 1;
                }
            }
            if (!foundIndex) index = GameModel.towerSet.Length;

            details.towerIndex = index;
            GameModel.towerSet = GameModel.towerSet.Insert(index, details);
            GameModel.childDependants.Add(details);

            TowerType.towers = TowerType.towers.Insert(index, details.towerId);

            GameModel.towers = GameModel.towers.Append(towers);
            GameModel.childDependants.Add(towers);

            GameModel.upgrades = GameModel.upgrades.Append(upgrades);
            GameModel.childDependants.Add(upgrades);

            GameModel.UpdateUpgradeNames();
        }

        /// <summary>
        /// Accesses the player's save when loaded, for unlocking towers and upgrades
        /// </summary>
        [HarmonyPatch(typeof(PlayerService<Btd6Player, ProfileModel>), nameof(PlayerService<Btd6Player, ProfileModel>.Load))]
        [HarmonyPostfix]
        public static void UnlockModdedTowers(Task<Btd6Player> __result) {
            __result.ContinueWith(new System.Action<Task<Btd6Player>>(t => {
                Btd6Player player = t.Result;
                ProfileModel profile = player.Data;

                profile.unlockedTowers.AddIfNotPresent(GSU.Name);
                profile.acquiredUpgrades.AddIfNotPresent(GSU.Names[1]);
                profile.acquiredUpgrades.AddIfNotPresent(GSU.Names[2]);
                profile.acquiredUpgrades.AddIfNotPresent(GSU.Names[3]);

                profile.highestSeenRound = 9999999;
            }), TaskScheduler.Default);
        }

        #region Assets

        /// <summary>
        /// For loading 3d models into the game
        /// </summary>
        [HarmonyPatch(typeof(Factory.__c__DisplayClass21_0), nameof(Factory.__c__DisplayClass21_0._CreateAsync_b__0))]
        [HarmonyPrefix]
        public static bool LoadModels(Factory.__c__DisplayClass21_0 __instance, UnityDisplayNode prototype) {
            string objectId = __instance.objectId.guidRef;
            if (!string.IsNullOrEmpty(objectId) && prototype is null) {
                return !GSU.LoadDisplay(objectId, __instance, new System.Action<UnityDisplayNode>(proto => {
                    SetUpPrototype(proto, __instance.objectId, __instance);
                    SetUpDisplay(proto, __instance);
                }));
            }
            return true;
        }
        private static void SetUpPrototype(UnityDisplayNode proto, PrefabReference protoRef, Factory.__c__DisplayClass21_0 assetFactory) {
            proto.transform.parent = assetFactory.__4__this.PrototypeRoot;
            proto.Active = false;
            proto.gameObject.transform.position = new Vector3(-3000, 0, 0);
            proto.gameObject.transform.eulerAngles = Vector3.zero;
            proto.cloneOf = protoRef;
            assetFactory.__4__this.prototypeHandles[protoRef] = Addressables.Instance.ResourceManager.CreateCompletedOperation(proto.gameObject, "");
        }
        private static void SetUpDisplay(UnityDisplayNode proto, Factory.__c__DisplayClass21_0 assetFactory) {
            UnityDisplayNode display = Object.Instantiate(proto.gameObject, assetFactory.__4__this.DisplayRoot).GetComponent<UnityDisplayNode>();

            display.transform.parent = assetFactory.__4__this.DisplayRoot;
            display.Active = true;
            display.cloneOf = proto.cloneOf;
            assetFactory.__4__this.active.Add(display);
            assetFactory.onComplete?.Invoke(display);
        }
        private static void SetUpAlreadyMadeDisplay(UnityDisplayNode display, Factory.__c__DisplayClass21_0 assetFactory) {
            display.Active = true;
            assetFactory.onComplete?.Invoke(display);
        }

        /// <summary>
        /// For loading sprites into the game
        /// </summary>
        [HarmonyPatch(typeof(SpriteAtlas), nameof(SpriteAtlas.GetSprite))]
        [HarmonyPrefix]
        public static bool LoadSprites(ref Sprite __result, string name) {
            if (!string.IsNullOrEmpty(name)) {
                Sprite sprite = GSU.LoadSprite(name);
                if (sprite is not null) {
                    __result = sprite;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// For highlighting the tower 3d model
        /// </summary>
        [HarmonyPatch(typeof(UnityDisplayNode), nameof(UnityDisplayNode.Hilight), MethodType.Setter)]
        [HarmonyPostfix]
        public static void SetHilight(ref UnityDisplayNode __instance, bool value) {
            if (__instance.genericRenderers.Length < 1)
                __instance.RecalculateGenericRenderers();
            if (__instance.genericRenderers.Length < 1) // Easy win
                return;

            Material material = __instance.genericRenderers[0].material;
            if (material.HasProperty($"_{GSU.Name}")) {
                material.SetFloat("_Selected", value ? 1 : 0);
                material.SetInt("_ZTest", (int)(value ? CompareFunction.Always : CompareFunction.Less));
            }
        }

        [HarmonyPatch(typeof(BuffIndicatorUi), nameof(BuffIndicatorUi.Awake))]
        [HarmonyPostfix]
        public static void MakeNewBuffIcons(BuffIndicatorUi __instance) {
            GSU.AddBuffIcon(__instance);
        }

        [HarmonyPatch(typeof(UpgradeScreen), nameof(UpgradeScreen.PopulatePaths))]
        [HarmonyPrefix]
        public static bool DoShitToUpgrades(UpgradeScreen __instance, TowerModel towerModel) {
            if (towerModel.baseId.Equals(GSU.Name)) {
                foreach (var u in __instance.path1Upgrades)
                    u.gameObject.SetActive(false);
                foreach (var u in __instance.path3Upgrades)
                    u.gameObject.SetActive(false);
                __instance.paragonPanel.SetActive(false);

                Transform bgArrows = __instance.transform.Find("BGArrows");
                bgArrows.GetChild(0).gameObject.SetActive(false);
                bgArrows.GetChild(2).gameObject.SetActive(false);

                for (int i = 0; i < __instance.path2Upgrades.Length; i++) {
                    UpgradeDetails upgrade = __instance.path2Upgrades[i];
                    if (i < 4 && i > 0) {
                        upgrade.SetUpgrade(GSU.Name, GameModel.upgradesByName[GSU.Names[i]],
                            new Il2CppSystem.Collections.Generic.List<AbilityModel>().Cast<Il2CppSystem.Collections.Generic.ICollection<AbilityModel>>(),
                            0, GameModel.GetTower(towerModel.baseId, i, 0, 0).portrait);
                        upgrade.SetPurchased(false);
                    } else {
                        for (int j = 0; j < upgrade.transform.childCount; j++)
                            upgrade.transform.GetChild(j).gameObject.SetActive(false);
                    }
                }

                __instance.SelectUpgrade(__instance.path2Upgrades[1]);

                return false;
            }
            return true;
        }

        #endregion
    }
}
