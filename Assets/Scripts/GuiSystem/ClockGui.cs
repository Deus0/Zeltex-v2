using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace AnimationUtilities {
	public class ClockGui : MonoBehaviour {
		public UnityEvent OnMinutePassed;
		public UnityEvent OnHourPassed;
		public UnityEvent OnDayPassed;

		public float PresetTime = 0;
		public Text MyDayText;
		public Text MyTimeText;
		public RectTransform MyStartPoint;
		public Material MyMaterial;
		private float SecondsHandLength = 0.32f;
		private float MinutesHandLength = 0.28f;
		private float HoursHandLength = 0.25f;
		private float SecondsHandWidth = 12f;
		private float MinutesHandWidth = 18f;
		private float HoursHandWidth = 24f;

		// for the things
		private LineRenderer MySecondsLine;
		private LineRenderer MyMinutesLine;
		private LineRenderer MyHourLine;
		private float DaysPassedTotal;
		private float SecondsPassedTotal;
		private float MinutesPassedTotal;
		private float HoursPassedTotal;
		private float SecondsPassed;
		private float MinutesPassed;
		private float HoursPassed;

		// Use this for initialization
		void Start () {
			SecondsHandLength = MyStartPoint.GetSize ().magnitude/3f;
			MinutesHandLength = SecondsHandLength*0.9f;
			HoursHandLength = MinutesHandLength*0.9f;

			SecondsHandLength *= transform.lossyScale.x;
			MinutesHandLength *= transform.lossyScale.x;
			HoursHandLength *= transform.lossyScale.x;
			MySecondsLine = CreateClockLine (SecondsHandWidth);
			MyMinutesLine = CreateClockLine (MinutesHandWidth);
			MyHourLine = CreateClockLine (HoursHandWidth);
			/*MyMinutesLine.SetWidth (SecondsHandWidth*transform.lossyScale.x, SecondsHandWidth*transform.lossyScale.x);
			MyMinutesLine.SetWidth (MinutesHandWidth*transform.lossyScale.x, MinutesHandWidth*transform.lossyScale.x);
			MyHourLine.SetWidth (HandWidth*transform.lossyScale.x, HandWidth*transform.lossyScale.x);*/
		}
		public LineRenderer CreateClockLine(float MyWidth) {
			GameObject MySecondsLineObject = new GameObject ();
			MySecondsLineObject.transform.SetParent (MyStartPoint);
			MySecondsLineObject.transform.position = MyStartPoint.transform.position;
			LineRenderer NewLine = MySecondsLineObject.AddComponent<LineRenderer> ();
			NewLine.SetVertexCount (2);
			NewLine.material = MyMaterial;
			NewLine.SetWidth (MyWidth*transform.lossyScale.x, MyWidth*transform.lossyScale.x/1.1f);
			NewLine.SetPosition (0, MyStartPoint.position);
			NewLine.SetPosition (1, MyStartPoint.position);
			return NewLine;
		}
		// Update is called once per frame
		void Update () 
		{
			SecondsPassedTotal = (PresetTime+Time.time);
			MinutesPassedTotal = (SecondsPassedTotal / 60f);
			HoursPassedTotal = MinutesPassedTotal / 60f;
			float DaysPassedTotal = HoursPassedTotal / 24f;

			SecondsPassed = SecondsPassedTotal-((int)MinutesPassedTotal)*60;
			MinutesPassed = MinutesPassedTotal-((int)HoursPassedTotal)*60;
			HoursPassed = HoursPassedTotal - ((int)DaysPassedTotal)*24;	//(Modulus days passed)
			
			MySecondsLine.SetPosition (0, MyStartPoint.position);
			MyMinutesLine.SetPosition (0, MyStartPoint.position);
			MyHourLine.SetPosition (0, MyStartPoint.position);

			MySecondsLine.SetPosition (1, MyStartPoint.position +
			                           SecondsHandLength*transform.up*Mathf.Sin(Mathf.PI/2f+(Mathf.PI*2f*(SecondsPassedTotal/60f)))+
			                           -SecondsHandLength*transform.right*Mathf.Cos(Mathf.PI/2f+(Mathf.PI*2f*(SecondsPassedTotal/60f))));
			MyMinutesLine.SetPosition (1, MyStartPoint.position +
			                           MinutesHandLength*transform.up*Mathf.Sin(Mathf.PI/2f+(Mathf.PI*2f*(MinutesPassed/60f)))+
			                           -MinutesHandLength*transform.right*Mathf.Cos(Mathf.PI/2f+(Mathf.PI*2f*(MinutesPassed/60f))));
			
			MyHourLine.SetPosition (1, MyStartPoint.position +
			                        HoursHandLength*transform.up*Mathf.Sin(Mathf.PI/2f+(Mathf.PI*2f*(HoursPassed/12f)))+
			                        -HoursHandLength*transform.right*Mathf.Cos(Mathf.PI/2f+(Mathf.PI*2f*(HoursPassed/12f))));

			string HoursPassedString = ((int)HoursPassed).ToString ();
			if (HoursPassedString.Length == 1)
				HoursPassedString = HoursPassedString.Insert (0, "0");

			string MinutesPassedString = ((int)MinutesPassed).ToString();
			if (MinutesPassedString.Length == 1)
				MinutesPassedString = MinutesPassedString.Insert (0, "0");
			
			string SecondsPassedString = ((int)SecondsPassed).ToString();
			if (SecondsPassedString.Length == 1)
				SecondsPassedString = SecondsPassedString.Insert (0, "0");

			if (MyTimeText)
				MyTimeText.text = HoursPassedString + ":" + MinutesPassedString + ":" + SecondsPassedString;
			if (MyDayText)
				MyDayText.text = "Day " +((int)(DaysPassedTotal+1)).ToString ();
		}


	}
}

//scraps


/*
 * // Draw a polygon in the XY plane with a specfied position, number of sides
// and radius.
void DebugDrawPolygon(Vector2 center, float radius, int numSides) {
	// The corner that is used to start the polygon (parallel to the X axis).
	Vector2 startCorner = new Vector2(radius, 0) + center;
	
	// The "previous" corner point, initialised to the starting corner.
	Vector2 previousCorner = startCorner;
	
	// For each corner after the starting corner...
	for (int i = 1; i < numSides; i++) {
		// Calculate the angle of the corner in radians.
		float cornerAngle = 2f * Mathf.PI / (float) numSides * i;
		
		// Get the X and Y coordinates of the corner point.
		Vector2 currentCorner = new Vector2(Mathf.Cos(cornerAngle) * radius, Mathf.Sin(cornerAngle) * radius) + center;
		
		// Draw a side of the polygon by connecting the current corner to the previous one.
		Debug.DrawLine(currentCorner, previousCorner);
		
		// Having used the current corner, it now becomes the previous corner.
		previousCorner = currentCorner;
	}
	
	// Draw the final side by connecting the last corner to the starting corner.
	Debug.DrawLine(startCorner, previousCorner);
}*/