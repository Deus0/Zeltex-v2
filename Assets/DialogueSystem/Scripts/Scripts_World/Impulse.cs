using UnityEngine;
using System.Collections;


namespace DialogueSystem {
	public class Impulse : MonoBehaviour {
		public float ForceRange = 10f;
		public AudioClip MyApplySound;
		private AudioSource MySource;

		public void ApplyImpulse() {
			Debug.Log ("Applying impulse!");

			if (MySource == null)
				MySource = gameObject.AddComponent<AudioSource> ();
			if (MyApplySound)
				MySource.PlayOneShot (MyApplySound);

			Rigidbody MyRigid = gameObject.GetComponent<Rigidbody> ();
			if (MyRigid) {
				MyRigid.AddForce(new Vector3(Random.Range(-ForceRange, ForceRange), Random.Range(-ForceRange, ForceRange), Random.Range(-ForceRange, ForceRange)));
			}
			GetComponent<ItemObject> ().Reset ();
		}
	}
}