using UnityEngine;
using System.Collections;

// Utility download

public class CameraMovement : MonoBehaviour 
{
	public float MoveSpeed = 20.0f; //regular speed
	//private float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
	private float maxShift = 1000.0f; //Maximum speed when holdin gshift
	private float camSens = 0.2f; //How sensitive it with mouse
	private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
	private float totalRun = 1.0f;


	void OnEnable() 
	{
		if (GetComponent<CharacterSystem.MouseLocker> ()) {
			GetComponent<CharacterSystem.MouseLocker> ().SetMouse(false);
		}
	}
	void Update ()
	{
		
		if (Input.GetMouseButtonDown (1)) 
		{
			lastMouse = Input.mousePosition;
		}
		if (Input.GetMouseButton (1)) 	// when holding down right click
		{
			lastMouse = Input.mousePosition - lastMouse;
			lastMouse = new Vector3 (-lastMouse.y * camSens, lastMouse.x * camSens, 0);
			lastMouse = new Vector3 (transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
			transform.eulerAngles = lastMouse;
			lastMouse = Input.mousePosition;
			//Mouse  camera angle done.  
		
			//Keyboard commands
			float f = 0.0f;
			var BaseInput = GetBaseInput ();
			var MouseScrollInput = GetInputMouseScroll();
			BaseInput = BaseInput * MoveSpeed;
			MouseScrollInput = MouseScrollInput * 0.5f;
			if (Input.GetKey (KeyCode.LeftShift)) 
			{
				BaseInput = BaseInput * 2f;
				MouseScrollInput = MouseScrollInput * 4f;
			} else {
			}
			if (Input.GetKey (KeyCode.E)) 
			{
				BaseInput.y += MoveSpeed;
			}
			else if (Input.GetKey (KeyCode.Q)) 
			{
				BaseInput.y -= MoveSpeed;
			}
		
			BaseInput = BaseInput * Time.deltaTime;
			if (Input.GetKey (KeyCode.Space)) 
			{ //If player wants to move on X and Z axis only
				f = transform.position.y;
				transform.Translate (BaseInput);
				transform.position = new Vector3 (transform.position.x, f, transform.position.z);
			}
			else 
			{
				transform.Translate (BaseInput);
				transform.Translate (MouseScrollInput);
				//transform.position += transform.InverseTransformPoint(BaseInput);
				//transform.position += transform.InverseTransformPoint(MouseScrollInput);
			}
		}
	}

	private Vector3 GetInputMouseScroll() 
	{
		Vector3 MyInput = new Vector3();
		float MouseScroll = Input.GetAxis ("Mouse ScrollWheel");
		if (MouseScroll > 0) {
			MyInput += new Vector3 (0, 0, 1);
		} else if (MouseScroll < 0) {
			MyInput += new Vector3 (0, 0, -1);
		}
		return MyInput;
	}

	//returns the basic values, if it's 0 than it's not active.
	private Vector3 GetBaseInput()
	{
		Vector3 MyInput = new Vector3();
		if (Input.GetKey (KeyCode.W)){
			MyInput += new Vector3(0, 0 , 1);
		}
		if (Input.GetKey (KeyCode.S)){
			MyInput += new Vector3(0, 0 , -1);
		}
		if (Input.GetKey (KeyCode.A)){
			MyInput += new Vector3(-1, 0 , 0);
		}
		if (Input.GetKey (KeyCode.D)){
			MyInput += new Vector3(1, 0 , 0);
		}
		return MyInput;
	}

}
