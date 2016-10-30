using UnityEngine;
using System.Collections;

public interface IDamageable
{
	void Damage(int damage);

    int StartingHealth { get; }

    int CurrentHealth { get; }

    bool IsDead { get; }

    bool BecomesPhysicsObjectOnDeath { get; }

    bool IsInWater { get; }

    bool IsCrashedOnGround { get; }
}
