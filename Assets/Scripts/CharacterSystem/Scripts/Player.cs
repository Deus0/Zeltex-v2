using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

/*
		Player class
			Any extra things on top of the Characters functionality
			Keyboard input

	To convert to player: - All other classes with input
		Toggle keys
		GuiManager Keys
		TestMovement, MouseLocker, MouseLook classes
		CameraMovement
*/

namespace CharacterSystem 
{
	public class Player : MonoBehaviour 
	{
		public bool IsDebugMode = false;
		public List<KeyCode> MyInteractKeys = new List<KeyCode> ();

		// Update is called once per frame
		void Update ()
		{
			HandleInput ();
		}
		void OnGUI () 
		{
			if (IsDebugMode) {
				// show key shortcuts
			}
		}
		private void HandleInput() 
		{
			if (MyInteractKeys.Count == 0)
			{
				// default keys
				MyInteractKeys.Add (KeyCode.Mouse0);
				MyInteractKeys.Add (KeyCode.Mouse1);
			}
			CombatSystem.Shooter MyShooter = GetComponent<CombatSystem.Shooter> ();
			for (int i = 0; i < MyInteractKeys.Count; i++)
			{
				if (Input.GetKeyDown(MyInteractKeys[i]))
				{
					if (i == 0 && MyShooter && MyShooter.enabled) 
					{
						MyShooter.Shoot();
					}
					gameObject.GetComponent<Character>().RayTraceSelections(i);
				}
			}
		}
	}
}
