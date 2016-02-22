using UnityEngine;
using System.Collections;

public class GravityZone : ArtificialGravity {

	public void OnCharacterEnter(GameObject MyCharacter) {
		if (MyCharacter.GetComponent<CharacterSystem.Character> ()) {
			enabled = true;
		}
	}
}
