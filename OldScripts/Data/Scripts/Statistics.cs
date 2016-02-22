using UnityEngine;
using System.Collections;

// Statistics for each hit the character takes
[System.Serializable]
public class HitStats {
	public int PlayerHitIndex;
	public float HitTime;
	public float DamageDone;
	public int ProjectileType;
	public bool IsFriend;
};