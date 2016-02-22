using UnityEngine;
using System.Collections;
using CharacterSystem;
using UnityEngine.Events;

namespace WorldUtilities {
	public class Impulse : MonoBehaviour {
		public MyEvent OnImpulse;
		public float ForceRange = 10f;
		public AudioClip MyApplySound;
		private AudioSource MySource;
		
		public void ApplyImpulse(GameObject WhereFrom) {
			ApplyImpulse (WhereFrom, false);
		}
		public void PullImpulse(GameObject WhereFrom) {
			ApplyImpulse (WhereFrom, true);
		}
		public void ApplyImpulse(GameObject WhereFrom, bool IsBackwards) {
			//Debug.LogError ("Applying impulse!");

			if (MySource == null)
				MySource = gameObject.AddComponent<AudioSource> ();
			if (MyApplySound)
				MySource.PlayOneShot (MyApplySound);

			Rigidbody MyRigid = gameObject.GetComponent<Rigidbody> ();
			if (MyRigid) {
				Vector3 Direction = transform.position-WhereFrom.transform.position;
				Direction.Normalize();
				if (IsBackwards)
					Direction *= -1f;
				MyRigid.AddForce(ForceRange*Direction);
				//MyRigid.AddForce(new Vector3(Random.Range(-ForceRange, ForceRange), Random.Range(-ForceRange, ForceRange), Random.Range(-ForceRange, ForceRange)));
			}
			if (OnImpulse != null) {
				OnImpulse.Invoke(WhereFrom);	// remember to link it to the ItemObject to reset the pickup!
			}
		}
	}
}