using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace GSU.Utils {
    internal static class ModelUtils {
        /// <summary>
        /// A methods that clones the given <see cref="Model"/> and casts it back to its original type
        /// </summary>
        /// <typeparam name="T">The original type of the <see cref="Model"/></typeparam>
        /// <param name="model">The <see cref="Model"/></param>
        /// <returns>A cloned <see cref="Model"/> of the same type</returns>
        public static T CloneCast<T>(this T model) where T : Model => model.Clone().Cast<T>();

        /// <summary>
        /// Finds the first behavior of the <see cref="TowerModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="tower">The <see cref="TowerModel"/></param>
        /// <returns>The found behavior</returns>
        public static T FirstBehavior<T>(this TowerModel tower) where T : Model => FirstBehavior<T, Model>(tower.behaviors);

        /// <summary>
        /// Finds the nth + 1 behavior of the <see cref="TowerModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="tower">The <see cref="TowerModel"/></param>
        /// <param name="n">The number of behaviors before this one</param>
        /// <returns>The found behavior</returns>
        public static T GetBehavior<T>(this TowerModel tower, int n) where T : Model => GetBehavior<T, Model>(tower.behaviors, n);

        /// <summary>
        /// Finds the first behavior of the <see cref="AttackModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="attack">The <see cref="AttackModel"/></param>
        /// <returns>The found behavior</returns>
        public static T FirstBehavior<T>(this AttackModel attack) where T : Model => FirstBehavior<T, Model>(attack.behaviors);

        /// <summary>
        /// Finds the nth + 1 behavior of the <see cref="AttackModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="attack">The <see cref="AttackModel"/></param>
        /// <param name="n">The number of behaviors before this one</param>
        /// <returns>The found behavior</returns>
        public static T GetBehavior<T>(this AttackModel attack, int n) where T : Model => GetBehavior<T, Model>(attack.behaviors, n);

        /// <summary>
        /// Finds the first behavior of the <see cref="WeaponModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="weapon">The <see cref="WeaponModel"/></param>
        /// <returns>The found behavior</returns>
        public static T FirstBehavior<T>(this WeaponModel weapon) where T : WeaponBehaviorModel => FirstBehavior<T, WeaponBehaviorModel>(weapon.behaviors);

        /// <summary>
        /// Finds the nth + 1 behavior of the <see cref="WeaponModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="weapon">The <see cref="WeaponModel"/></param>
        /// <param name="n">The number of behaviors before this one</param>
        /// <returns>The found behavior</returns>
        public static T GetBehavior<T>(this WeaponModel weapon, int n) where T : WeaponBehaviorModel => GetBehavior<T, WeaponBehaviorModel>(weapon.behaviors, n);

        /// <summary>
        /// Finds the first behavior of the <see cref="ProjectileModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="projectile">The <see cref="ProjectileModel"/></param>
        /// <returns>The found behavior</returns>
        public static T FirstBehavior<T>(this ProjectileModel projectile) where T : Model => FirstBehavior<T, Model>(projectile.behaviors);

        /// <summary>
        /// Finds the nth + 1 behavior of the <see cref="ProjectileModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="projectile">The <see cref="ProjectileModel"/></param>
        /// <param name="n">The number of behaviors before this one</param>
        /// <returns>The found behavior</returns>
        public static T GetBehavior<T>(this ProjectileModel projectile, int n) where T : Model => GetBehavior<T, Model>(projectile.behaviors, n);

        /// <summary>
        /// Finds the first behavior of the <see cref="AbilityModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="ability">The <see cref="AbilityModel"/></param>
        /// <returns>The found behavior</returns>
        public static T FirstBehavior<T>(this AbilityModel ability) where T : Model => FirstBehavior<T, Model>(ability.behaviors);

        /// <summary>
        /// Finds the nth + 1 behavior of the <see cref="AbilityModel"/> of the specified type
        /// </summary>
        /// <typeparam name="T">The type to be found</typeparam>
        /// <param name="ability">The <see cref="AbilityModel"/></param>
        /// <param name="n">The number of behaviors before this one</param>
        /// <returns>The found behavior</returns>
        public static T GetBehavior<T>(this AbilityModel ability, int n) where T : Model => GetBehavior<T, Model>(ability.behaviors, n);

        private static T FirstBehavior<T, B>(Il2CppReferenceArray<B> behaviors) where T : B where B : Model {
            foreach (Model behavior in behaviors) {
                if (behavior is not null && Il2CppType.Of<T>().IsAssignableFrom(behavior.GetIl2CppType()))
                    return behavior.Cast<T>();
            }
            return null;
        }

        private static T GetBehavior<T, B>(Il2CppReferenceArray<B> behaviors, int n) where T : B where B : Model {
            int i = 0;
            foreach (Model behavior in behaviors) {
                if (Il2CppType.Of<T>().IsAssignableFrom(behavior.GetIl2CppType())) {
                    if (i == n)
                        return behavior.Cast<T>();
                    i++;
                }
            }
            return null;
        }
    }
}
