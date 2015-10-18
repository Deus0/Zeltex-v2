using UnityEngine;
using System.Collections;

public class SunCycle : MonoBehaviour {
	public Material DayMaterial;
	public Material NightMaterial;
	public Light MyDirectionalLight;
	public bool IsDayTime = false;
	public float HoursPerDay = 0;
	public float MinutesPerDay = 0;
	public float SecondsPerDay = 15;
	float TimeStarted;
	float LastDayPassed = 0;
	public bool IsChangingTime = false;
	// animate directional light
	public Vector3 MyTargetDirection;
	public Vector3 FromDirection;
	public float TimeStartedToLerp;
	public float TimeToAnimate = 5f;

	void Start() {
		TimeStarted = Time.time;
		LastDayPassed = Time.time;
	}
	void Update () {
		if (HasDayPassed ()) {
			ChangeSky ();
		}
		if (IsChangingTime) {
			AnimateLightDirection();
		}
	}
	public void AnimateLightDirection() {
		MyDirectionalLight.transform.eulerAngles = Vector3.Lerp(FromDirection, MyTargetDirection, (1f/TimeToAnimate)*(Time.time-TimeStartedToLerp));
		if (Time.time-TimeStartedToLerp > TimeToAnimate) {
			IsChangingTime = false;
		}
	}
	public void DayTime() {
		if (!IsDayTime) {
			Debug.Log("Changing to day time.");
			LastDayPassed = Time.time-SecondsPerDayTotal();
		}
	}
	public void NightTime() {
		if (IsDayTime) {
			Debug.Log("Changing to night time.");
			LastDayPassed = Time.time-SecondsPerDayTotal();
		}
	}
	public float SecondsPerDayTotal() {
		return HoursPerDay * 60f * 60f + MinutesPerDay * 60 + SecondsPerDay;
	}
	public bool HasDayPassed() {
		if (Time.time - LastDayPassed >= SecondsPerDayTotal()) {
			LastDayPassed = Time.time;
			return true;
		} else {
			return false;
		}
	}
	private void ChangeSky()
	{
		if (IsDayTime)
		{
			IsDayTime = false;
			//DayMaterial = RenderSettings.skybox;
			//RenderSettings.skybox = NightMaterial;
			FromDirection = new Vector3(90,0,0);
			MyTargetDirection = new Vector3(-90,0,0);
			TimeStartedToLerp = Time.time;
			IsChangingTime = true;
		}
		else if (!IsDayTime) {
			IsDayTime = true;
			//RenderSettings.skybox = DayMaterial;
			MyTargetDirection = new Vector3(90,0,0);
			FromDirection = new Vector3(-90,0,0);
			TimeStartedToLerp = Time.time;
			IsChangingTime = true;
		}
	}
}