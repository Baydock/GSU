using GSU.Utils;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.TowerFilters;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using NKVector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;
using Resources = GSU.Properties.Resources;

namespace GSU {
    internal static class GSU {
        public const string Name = "GSU";
        private const string SentryName = "GSUSentry";
        private const string TechCrystalName = "GSUTechCrystal";
        public static string[] Names { get; } = {
            Name,
            "Ultimate Sentries",
            "Ultimate Tech-Crystals",
            "Ultimate Link"
        };

        public const string After = "DartlingGunner";

        public const TowerSet Set = TowerSet.Military;

        private const string ResourcePrefix = $"{Name}.";
        private const string PortraitPrefix = "Portraits.";
        private const string UpgradeIconPrefix = "UpgradeIcons.";
        private const string TexturePrefix = "Textures.";

        private const int Cost = 100000;
        private const int Radius = 11;

        private static readonly AssetBundle AssetBundle = Resources.LoadAssetBundle("gsu");

        #region Upgrades and Towers

        public static ShopTowerDetailsModel Details => new(Name, -1, 3, 0, 0, -1, 0);
        public static TowerModel Tower0 => GetTower(0);

        public static UpgradeModel Upgrade1 => GetUpgrade(1, 1000000);
        public static TowerModel Tower1 => GetTower(1);

        public static UpgradeModel Upgrade2 => GetUpgrade(2, 2000000);
        public static TowerModel Tower2 => GetTower(2);

        public static UpgradeModel Upgrade3 => GetUpgrade(3, 3000000);
        public static TowerModel Tower3 => GetTower(3);

        #endregion

        private static TowerModel GetTower(int tier) {
            string name = tier == 0 ? Name : $"{Name}-{tier}00";
            string display = $"{ResourcePrefix}{tier}";

            string portrait = tier.ToString();
            portrait = $"Ui[{ResourcePrefix}{PortraitPrefix}{portrait}]";

            List<string> appliedUpgrades = new();
            for (byte i = 1; i <= tier; i++)
                appliedUpgrades.Add(Names[i]);

            TowerModel bombShooter = Mod.GameModel.GetTowerFromId(TowerType.BombShooter);
            TowerModel monkeySub030 = Mod.GameModel.GetTower(TowerType.MonkeySub, 0, 3, 0);
            TowerModel engineerMonkey200 = Mod.GameModel.GetTower(TowerType.EngineerMonkey, 2, 0, 0);
            TowerModel druid200 = Mod.GameModel.GetTower(TowerType.Druid, 2, 0, 0);
            TowerModel ninjaMonkey030 = Mod.GameModel.GetTower(TowerType.NinjaMonkey, 0, 3, 0);
            TowerModel etienne20 = Mod.GameModel.GetTowerWithName(TowerType.Etienne + " 20");
            TowerModel dartlingGunner500 = Mod.GameModel.GetTower(TowerType.DartlingGunner, 5, 0, 0);
            TowerModel adoraBallOfLight = Mod.GameModel.GetTowersWithBaseId(TowerType.AdoraBallOfLight)[0];

            TowerModel gsu = bombShooter.CloneCast();
            gsu.name = name;
            gsu.baseId = Name;
            gsu.towerSet = Set;
            gsu.display = new() { guidRef = display };
            gsu.cost = Cost;
            gsu.radius = Radius;
            gsu.range = 20;
            gsu.isGlobalRange = true;
            gsu.ignoreBlockers = true;
            gsu.tier = tier;
            gsu.tiers = new Il2CppStructArray<int>(3) { [0] = tier, [1] = 0, [2] = 0 };
            gsu.appliedUpgrades = appliedUpgrades.ToArray().Cast<Il2CppStringArray>();
            gsu.upgrades = tier == 3 ? new Il2CppReferenceArray<UpgradePathModel>(0) : new Il2CppReferenceArray<UpgradePathModel>(1) { [0] = new UpgradePathModel(Names[tier + 1], $"{Name}-{tier + 1}00") };
            //gsu.areaTypes = new Il2CppStructArray<AreaType>(1) { [0] = AreaType.land };
            gsu.icon = new() { guidRef = $"Ui[{ResourcePrefix}{PortraitPrefix}0]" };
            gsu.portrait = new() { guidRef = portrait };
            gsu.mods = new Il2CppReferenceArray<ApplyModModel>(0);
            gsu.footprint = new CircleFootprintModel("", Radius, false, false, false);
            List<Model> gsuBehaviors = new();
            gsuBehaviors.Add(
                gsu.FirstBehavior<CreateSoundOnTowerPlaceModel>(),
                gsu.FirstBehavior<CreateSoundOnSellModel>(),
                gsu.FirstBehavior<CreateSoundOnUpgradeModel>(),
                gsu.FirstBehavior<CreateEffectOnPlaceModel>(),
                gsu.FirstBehavior<CreateEffectOnSellModel>(),
                gsu.FirstBehavior<CreateEffectOnUpgradeModel>(),
                new SyncTargetPriorityWithSubTowersModel("", false, SentryName, ""),
                monkeySub030.GetBehavior<AttackModel>(1).CloneCast());
            if (tier > 0)
                gsuBehaviors.Add(engineerMonkey200.FirstBehavior<AttackModel>().CloneCast());
            if (tier > 1)
                gsuBehaviors.Add(etienne20.GetBehavior<AttackModel>(1).CloneCast());
            gsuBehaviors.Add(gsu.FirstBehavior<DisplayModel>());
            gsu.behaviors = gsuBehaviors.ToArray().Cast<Il2CppReferenceArray<Model>>();

            #region Missile

            AttackModel missileAttack = gsu.FirstBehavior<AttackModel>();
            missileAttack.range = 9999999;
            missileAttack.attackThroughWalls = true;
            missileAttack.behaviors = new Il2CppReferenceArray<Model>(4) {
                [0] = new TargetFirstModel("", true, false),
                [1] = new TargetLastModel("", true, false),
                [2] = new TargetCloseModel("", true, false),
                [3] = new TargetStrongModel("", true, false)
            };

            WeaponModel missileWeapon = missileAttack.weapons[0];
            missileWeapon.rate = 0.2f;
            missileWeapon.behaviors = null;

            ProjectileModel missileExplosion = missileWeapon.projectile.FirstBehavior<CreateProjectileOnExpireModel>().projectile;
            missileExplosion.filters = new Il2CppReferenceArray<FilterModel>(0);
            missileExplosion.behaviors = new Il2CppReferenceArray<Model>(3) {
                [0] = missileExplosion.FirstBehavior<DamageModel>(),
                [1] = missileExplosion.FirstBehavior<AgeModel>(),
                [2] = missileExplosion.FirstBehavior<DisplayModel>()
            };

            DamageModel missileDamage = missileExplosion.FirstBehavior<DamageModel>();
            missileDamage.damage = 1000;
            missileDamage.immuneBloonProperties = missileDamage.immuneBloonPropertiesOriginal = BloonProperties.None;

            #endregion

            #region Sentries

            if (tier > 0) {
                AttackModel sentryCreatingAttack = gsu.GetBehavior<AttackModel>(1);
                sentryCreatingAttack.attackThroughWalls = true;
                sentryCreatingAttack.targetProvider = new RandomPositionModel("", 20, 60, 6, false, 6, true, false, "Land", true, false, 43, "");
                sentryCreatingAttack.behaviors = new Il2CppReferenceArray<Model>(1) {
                    [0] = sentryCreatingAttack.targetProvider
                };

                WeaponModel sentryCreatingWeapon = sentryCreatingAttack.weapons[0];
                if (tier == 3)
                    sentryCreatingWeapon.rate /= 2;

                ProjectileModel sentryCreatingProjectile = sentryCreatingWeapon.projectile;
                sentryCreatingProjectile.display = new() { guidRef = null };
                sentryCreatingProjectile.behaviors = new Il2CppReferenceArray<Model>(5) {
                    [0] = missileWeapon.projectile.FirstBehavior<AgeModel>().CloneCast(),
                    [1] = missileWeapon.projectile.FirstBehavior<InstantModel>().CloneCast(),
                    [2] = missileWeapon.projectile.FirstBehavior<CreateEffectProjectileAfterTimeModel>().CloneCast(),
                    [3] = sentryCreatingProjectile.FirstBehavior<CreateTowerModel>(),
                    [4] = sentryCreatingProjectile.FirstBehavior<DisplayModel>()
                };

                CreateEffectProjectileAfterTimeModel sentryDropEffect = sentryCreatingProjectile.FirstBehavior<CreateEffectProjectileAfterTimeModel>();
                sentryDropEffect.effectModel = sentryDropEffect.effectModel.CloneCast();
                sentryDropEffect.effectModel.assetId = new() { guidRef = $"{ResourcePrefix}ballisticsentry" };

                DisplayModel sentryCreatingProjectileDisplay = sentryCreatingProjectile.FirstBehavior<DisplayModel>();
                sentryCreatingProjectileDisplay.display = sentryCreatingProjectile.display;

                TowerModel sentry = sentryCreatingProjectile.FirstBehavior<CreateTowerModel>().tower;
                sentry.name = SentryName;
                sentry.baseId = sentry.name;
                sentry.towerSet = Set;
                sentry.display = new() { guidRef = $"{ResourcePrefix}sentry" };
                sentry.range = 175;
                sentry.isGlobalRange = true;
                sentry.ignoreBlockers = true;
                sentry.portrait = new() { guidRef = $"Ui[{ResourcePrefix}{PortraitPrefix}sentry]" };
                sentry.mods = new Il2CppReferenceArray<ApplyModModel>(0);
                sentry.behaviors = new Il2CppReferenceArray<Model>(11) {
                    [0] = sentry.FirstBehavior<TowerExpireModel>(),
                    [1] = sentry.FirstBehavior<CircleFootprintModel>(),
                    [2] = sentry.FirstBehavior<SavedSubTowerModel>(),
                    [3] = sentry.FirstBehavior<CreateEffectOnPlaceModel>(),
                    [4] = sentry.FirstBehavior<Il2CppAssets.Scripts.Models.Towers.Behaviors.CreateEffectOnExpireModel>(),
                    [5] = sentry.FirstBehavior<CreateEffectOnSellModel>(),
                    [6] = sentry.FirstBehavior<CreditPopsToParentTowerModel>(),
                    [7] = sentry.FirstBehavior<CreateSoundOnTowerPlaceModel>(),
                    [8] = druid200.FirstBehavior<AttackModel>().CloneCast(),
                    [9] = ninjaMonkey030.FirstBehavior<SupportShinobiTacticsModel>().CloneCast(),
                    [10] = sentry.FirstBehavior<DisplayModel>()
                };

                AttackModel sentryAttack = sentry.FirstBehavior<AttackModel>();
                sentryAttack.range = 9999999;
                sentryAttack.attackThroughWalls = true;
                sentryAttack.behaviors = new Il2CppReferenceArray<Model>(4) {
                    [0] = new TargetFirstModel("", true, false),
                    [1] = new TargetLastModel("", true, false),
                    [2] = new TargetCloseModel("", true, false),
                    [3] = new TargetStrongModel("", true, false)
                };
                WeaponModel[] oldSentryWeapons = sentryAttack.weapons;
                sentryAttack.weapons = new Il2CppReferenceArray<WeaponModel>(1) { [0] = oldSentryWeapons[1] };

                WeaponModel sentryWeapon = sentryAttack.weapons[0];
                sentryWeapon.rate = 0.2f;
                sentryWeapon.ejectX = 0;
                sentryWeapon.ejectY = 0;
                sentryWeapon.ejectZ = 10;

                ProjectileModel sentryProjectile = sentryWeapon.projectile;
                sentryProjectile.filters = new Il2CppReferenceArray<FilterModel>(0);
                sentryProjectile.behaviors = new Il2CppReferenceArray<Model>(4) {
                    [0] = sentryProjectile.FirstBehavior<DamageModel>(),
                    [1] = sentryProjectile.FirstBehavior<CreateLightningEffectModel>(),
                    [2] = sentryProjectile.FirstBehavior<LightningModel>(),
                    [3] = sentryProjectile.FirstBehavior<DisplayModel>()
                };

                DamageModel sentryDamage = sentryProjectile.FirstBehavior<DamageModel>();
                sentryDamage.damage = 1000;
                sentryDamage.immuneBloonProperties = sentryDamage.immuneBloonPropertiesOriginal = BloonProperties.None;

                LightningModel sentryLightning = sentryProjectile.FirstBehavior<LightningModel>();
                sentryLightning.splitRange = 9999999;
                sentryLightning.splits = 50;

                SupportShinobiTacticsModel sentrySupport = sentry.FirstBehavior<SupportShinobiTacticsModel>();
                sentrySupport.multiplier = 0.1f;
                sentrySupport.filters = new Il2CppReferenceArray<TowerFilterModel>(1) {
                    [0] = new FilterInBaseTowerIdModel("", new Il2CppStringArray(2) {
                        [0] = SentryName,
                        [1] = TechCrystalName
                    })
                };
                sentrySupport.maxStacks = 9999999;
                sentrySupport.maxStackSize = 9999999;
                sentrySupport.mutatorId = "GSUSupport";
                sentrySupport.buffIconName = "";
                sentrySupport.buffLocsName = "GSUSentrySupportBuff";
                sentrySupport.isGlobalRange = true;

                BuffIndicatorModel sentryBuff = sentrySupport.childDependants[0].Cast<BuffIndicatorModel>();
                sentryBuff.buffName = sentrySupport.buffLocsName;
                sentryBuff.globalRange = true;
                sentryBuff.iconName = sentrySupport.buffIconName;
                sentryBuff.maxStackSize = sentrySupport.maxStackSize;

                DisplayModel sentryDisplay = sentry.FirstBehavior<DisplayModel>();
                sentryDisplay.display = sentry.display;

                if (tier == 1)
                    Mod.GameModel.towers = Mod.GameModel.towers.Append(sentry);
            }

            #endregion

            #region Tech-Crystals

            if (tier > 1) {
                WeaponModel techCrystalCreatingWeapon = gsu.GetBehavior<AttackModel>(2).weapons[0];
                techCrystalCreatingWeapon.rate = tier == 3 ? 1.25f : 2.5f;
                techCrystalCreatingWeapon.behaviors = new Il2CppReferenceArray<WeaponBehaviorModel>(1) { [0] = new SubTowerFilterModel("", TechCrystalName, tier == 3 ? 8 : 4, false) };

                TowerModel techCrystal = techCrystalCreatingWeapon.projectile.FirstBehavior<CreateTowerModel>().tower = adoraBallOfLight.CloneCast();
                techCrystal.baseId = TechCrystalName;
                techCrystal.name = TechCrystalName;
                techCrystal.mods = new Il2CppReferenceArray<ApplyModModel>(0);
                techCrystal.display = new() { guidRef = $"{ResourcePrefix}techcrystal{tier}" };
                techCrystal.behaviors = new Il2CppReferenceArray<Model>(7) {
                    [0] = techCrystal.FirstBehavior<TowerExpireOnParentDestroyedModel>(),
                    [1] = new TowerExpireOnParentUpgradedModel("", tier),
                    [2] = techCrystal.FirstBehavior<CreditPopsToParentTowerModel>(),
                    [3] = techCrystal.FirstBehavior<IgnoreTowersBlockerModel>(),
                    [4] = techCrystal.FirstBehavior<OrbitingTowerModel>(),
                    [5] = techCrystal.FirstBehavior<AttackModel>(),
                    [6] = techCrystal.FirstBehavior<DisplayModel>()
                };

                OrbitingTowerModel techCrystalOrbit = techCrystal.FirstBehavior<OrbitingTowerModel>();
                techCrystalOrbit.rotationDegreesPerFrame = 0.6f;
                techCrystalOrbit.dontUseParentOrigin = true;
                techCrystalOrbit.offset = NKVector2.zero;
                techCrystalOrbit.radius = 120;

                AttackModel techCrystalAttack = techCrystal.FirstBehavior<AttackModel>();
                techCrystalAttack.behaviors = new Il2CppReferenceArray<Model>(5) {
                    [0] = new RotateToTargetModel("", false, false, false, 0, true, false),
                    [1] = new CirclePatternFirstModel("", 120, true, true, new(), 0, true),
                    [2] = new CirclePatternLastModel("", 120, true, true, new(), 0, true),
                    [3] = new CirclePatternCloseModel("", 120, true, true, new(), 0, true),
                    [4] = new CirclePatternStrongModel("", 120, true, true, new(), 0, true)
                };

                WeaponModel techCrystalWeapon = techCrystalAttack.weapons[0];
                techCrystalWeapon.Rate = 1 / 60f;

                LineProjectileEmissionModel techCrystalLine = techCrystalWeapon.emission.Cast<LineProjectileEmissionModel>();
                techCrystalLine.displayPath.assetPath = dartlingGunner500.FirstBehavior<AttackModel>().weapons[0].FirstBehavior<LineEffectModel>().lineDisplayPath.assetPath;
                techCrystalLine.displayLifetime = 1 / 60f;
                techCrystalLine.useTargetAsEndPoint = false;

                ProjectileModel techCrystalProjectile = techCrystalWeapon.projectile;
                techCrystalProjectile.pierce = 9999999;
                techCrystalProjectile.radius = 6;
                techCrystalProjectile.filters = new Il2CppReferenceArray<FilterModel>(0);
                techCrystalProjectile.behaviors = new Il2CppReferenceArray<Model>(3) {
                    [0] = techCrystalProjectile.FirstBehavior<DamageModel>(),
                    [1] = techCrystalProjectile.FirstBehavior<AgeModel>(),
                    [2] = techCrystalProjectile.FirstBehavior<DisplayModel>()
                };

                DamageModel techCrystalDamage = techCrystalProjectile.FirstBehavior<DamageModel>();
                techCrystalDamage.damage = 5000;

                DisplayModel techCrystalDisplay = techCrystal.FirstBehavior<DisplayModel>();
                techCrystalDisplay.display = techCrystal.display;
            }

            #endregion

            DisplayModel displayModel = gsu.FirstBehavior<DisplayModel>();
            displayModel.display = gsu.display;

            return gsu;
        }

        private static UpgradeModel GetUpgrade(byte tier, int cost) => new(Names[tier], cost, 0, new() { guidRef = $"Ui[{ResourcePrefix}{UpgradeIconPrefix}{tier}]" }, 0, tier, 0, "", "");

        /// <summary>
        /// Loads the model for the given name
        /// </summary>
        /// <param name="name">The name of the model</param>
        /// <returns>True if the display was loaded, false otherwise.</returns>
        public static bool LoadDisplay(string name, Factory.__c__DisplayClass21_0 assetFactory, System.Action<UnityDisplayNode> onComplete) {
            if (IsResource(name, out string resourceName)) {
                Object asset = AssetBundle.LoadAsset(resourceName, Il2CppType.Of<GameObject>());
                if (asset is not null) {
                    GameObject proto = asset.Cast<GameObject>();
                    GameObject obj = Object.Instantiate(proto);
                    obj.name = Name + obj.name;
                    obj.transform.position = new Vector3(-3000, 0, 0);

                    if (resourceName.StartsWith("techcrystal")) {
                        assetFactory.__4__this.CreateAsync(new() { guidRef = "d4bb987eab0baae449e08d95af67ff21" }, DisplayCategory.Tower, new System.Action<UnityDisplayNode>(udn => {
                            udn.cloneOf = new() { guidRef = null };
                            Transform laserEnergy = udn.transform;
                            laserEnergy.parent = obj.transform;
                            laserEnergy.localPosition = Vector3.zero;
                            laserEnergy.localEulerAngles = Vector3.zero;
                            laserEnergy.gameObject.name = "TechCrystalLaser";

                            Transform laserEnergyOffset = laserEnergy.Find("OffSet");
                            laserEnergyOffset.localPosition = new Vector3(0, 0, 13.8f);

                            Transform rightLightning = laserEnergyOffset.Find("DartlingLightning (2)");
                            Transform leftLightning = laserEnergyOffset.Find("DartlingLightning (3)");
                            Vector3 rightLightningPos = rightLightning.localPosition;
                            Vector3 leftLightningPos = leftLightning.localPosition;
                            rightLightningPos.z = leftLightningPos.z = -11.5f;
                            rightLightning.localPosition = rightLightningPos;
                            leftLightning.localPosition = leftLightningPos;

                            Object.DestroyImmediate(laserEnergyOffset.Find("DartlingLightning").gameObject);
                            Object.DestroyImmediate(laserEnergyOffset.Find("DartlingLightning (1)").gameObject);
                            Object.DestroyImmediate(laserEnergyOffset.Find("Plane (1)").gameObject);
                        }));
                    }

                    onComplete?.Invoke(obj.AddComponent<UnityDisplayNode>());
                } else {
                    if (resourceName.Equals("ballisticsentry")) {
                        assetFactory.__4__this.CreateAsync(new() { guidRef = "6e987400b060cd648879c9318834b18f" }, DisplayCategory.Projectile, new System.Action<UnityDisplayNode>(udn => {
                            udn.cloneOf = new() { guidRef = null };
                            udn.gameObject.name = "BallisticSentryFXDown";

                            Transform ballisticSentry = udn.transform.GetChild(0);
                            ballisticSentry.gameObject.name = "BallisticSentry";

                            ParticleSystemRenderer ballisticSentryParticle = ballisticSentry.GetComponent<ParticleSystemRenderer>();
                            ballisticSentryParticle.material.mainTexture = Resources.LoadTexture($"{TexturePrefix}ballisticsentry");

                            onComplete?.Invoke(udn);
                        }));
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Loads the <see cref="Sprite"/> for the given name
        /// </summary>
        /// <param name="name">The name of the <see cref="Sprite"/></param>
        /// <returns>The <see cref="Sprite"/> that was loaded, null if not found</returns>
        public static Sprite LoadSprite(string name) => IsResource(name, out string resourceName) ? Resources.LoadSprite(resourceName) : null;

        private static bool IsResource(string name, out string resourceName) {
            if (name.StartsWith(ResourcePrefix)) {
                resourceName = name[ResourcePrefix.Length..];
                return true;
            }
            resourceName = null;
            return false;
        }

        public static bool IsResource(string name) => name.StartsWith(ResourcePrefix);

        public static void AddBuffIcon(BuffIndicatorUi buffUi) {
            GameObject buffIconGameObject = Object.Instantiate(buffUi.buffPrefab, buffUi.buffsUi.transform.Find("Container"));
            buffIconGameObject.name = "GSUSentrySupportBuff";
            BuffIcon buffIcon = buffIconGameObject.GetComponent<BuffIcon>();
            buffIcon.inactiveIcon.sprite = LoadSprite($"{ResourcePrefix}{PortraitPrefix}sentry");
            buffIcon.activeIcon.sprite = buffIcon.inactiveIcon.sprite;
            buffIcon.Name = buffIcon.name = buffIconGameObject.name;

            buffUi.allBuffIcons.Add(buffIcon.Name, buffIcon);
        }

        public static void AddLocalization() {
            Dictionary<string, string> table = LocalizationManager.Instance.defaultTable;
            table.AddIfNotPresent(Name, "G.S.U");
            table.AddIfNotPresent(Name + " Description", "The Global Strike Unit is a powerful controller of all the mass destruction available.");
            table.AddIfNotPresent(Names[1] + " Description", "Adds super powerful lightning sentries that buff eachother.");
            table.AddIfNotPresent(Names[2] + " Description", "Adds super powerful tech-crystals that float around that shoot powerfully destructive lasers.");
            table.AddIfNotPresent(Names[3] + " Description", "Even more sentries and tech-crystals.");
            table.AddIfNotPresent(SentryName, "G.S.U Sentry");
        }
    }
}
