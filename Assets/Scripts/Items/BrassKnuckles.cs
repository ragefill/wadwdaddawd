using UnityEngine;

namespace GoreSandbox
{
    /// <summary>
    /// Equip on a hand to multiply punch damage and change damage type to blunt impact.
    </summary>
    public class BrassKnuckles : MonoBehaviour
    {
        public float multiplier = 2.2f;
        public float damageBonus = 8f;

        void OnValidate()
        {
            var w = GetComponent<Weapon>();
            if (w != null)
            {
                w.damageType = DamageType.Blunt;
                w.baseDamage = damageBonus;
            }
        }
    }
}
