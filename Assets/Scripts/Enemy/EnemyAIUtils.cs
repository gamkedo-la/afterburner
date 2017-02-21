using UnityEngine;
using System.Collections;

public static class EnemyAIUtils
{
	public static FlyingControl playerFlyingStats;

	public static float StandardiseAngle(float angle)
	{
		float newAngle = (360f + angle) % 360f;

		if(newAngle > 180f)
			newAngle -= 360f;

		return newAngle;
	}

	//Credit for the next two functions goes to the folks at Howling Moon Software
	public static Vector3 calculateLeadDirection(Vector3 playerDirection, float bulletSpeed, float decisionRate)
	{
		//Aim in front of player to lead shot
		float timeToImpact = AimAhead(playerDirection, playerFlyingStats.ForwardVelocityInWorld, bulletSpeed);

		//If there is a possible collision time in the future rather than the past
		if(timeToImpact > 0f)
		{
			//Return point to aim at reletive to gun barrels
			return (playerDirection + (timeToImpact + decisionRate) * playerFlyingStats.ForwardVelocityInWorld);
			//return answer;
		}
		//Else return the zero vector
		return Vector3.zero;
	}

	// Calculate the time when we can hit a target with a bullet
	// Return a negative time if there is no solution
	private static float AimAhead(Vector3 delta, Vector3 vr, float muzzleV)
	{
		// Quadratic equation coefficients a*t^2 + b*t + c = 0
		float a = Vector3.Dot(vr, vr) - muzzleV * muzzleV;
		float b = 2f * Vector3.Dot(vr, delta);
		float c = Vector3.Dot(delta, delta);

		float det = b * b - 4f * a * c;

		// If the determinant is negative, then there is no solution
		if(det > 0f)
		{
			return 2f * c / (Mathf.Sqrt(det) - b);
		}
		else
		{
			return -1f;
		}
	}
}