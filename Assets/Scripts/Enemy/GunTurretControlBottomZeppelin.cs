using UnityEngine;
using System.Collections;

public class GunTurretControlBottomZeppelin : GunTurretControl {

	protected override void Update ()
	{
		var previousOrientation = m_horizontalPivotPoint.localEulerAngles;
    base.Update();
		if(m_horizontalPivotPoint.localEulerAngles.y > 140 && m_horizontalPivotPoint.localEulerAngles.y < 220)
		{
			m_horizontalPivotPoint.localEulerAngles = previousOrientation;
		}
	}
}
