using UnityEngine;
using System.Collections;

namespace OldCode {
public class Enemy : BaseCharacter {
	// Use this for initialization

	public void ShootProjectilesBot() {
		base.ShootProjectiles ();
		/*ShootPosition = transform.position;
		ShootForwardVector = transform.forward;
		SpawnRotationQuaternion = transform.rotation;*/
	}
	// Update is called once per frame
	void Update () {
		AnimateColor();
		if (!MyStats.IsDead) {
			base.UpdateStats ();
			UpdateStats ();
			ShootProjectilesBot ();
		}
	}
		
	}
	
}
