using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ParentSpecial : MonoBehaviour {

	public Transform parent;

	[Header("Position Constraints")]
	public bool parentPositionX;
	public bool parentPositionY;
	public bool parentPositionZ;

	void Update ()
	{
		Vector3 newPos = transform.position;

		if(parentPositionX)
		{
			newPos.x = parent.transform.position.x;
		}

		if(parentPositionY)
		{
			newPos.y = parent.transform.position.y;
		}

		if(parentPositionZ)
		{
			newPos.z = parent.transform.position.z;
		}

		transform.position = newPos;
	
	}
}
