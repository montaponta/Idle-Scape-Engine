using System;
using UnityEngine;

public class Timer
{
	private float duration = -1;
	private DateTime startTime, endTime;
	private TimeSpan span;
	public Action OnTimerReached;
	public bool isTimerActive { get; private set; }
	public bool isTimerDone { get; private set; }
	private TimerMode timerMode;
	public int timerCounter;
	private bool loop, unsubscribeAllActions;
	public int count = 1;
	private bool isTimerPaused;

	public Timer(TimerMode mode, bool unsubscribeActionsAfterFinish = true, bool loop = false)
	{
		timerMode = mode;
		this.loop = loop;
		this.unsubscribeAllActions = unsubscribeActionsAfterFinish;
	}

	private void StartTimer(float duration, bool isResetTimerCounter = true)
	{
		this.duration = duration;
		endTime = startTime.AddSeconds(duration);
		span = endTime - DateTime.UtcNow;
		isTimerActive = true;
		if (timerCounter == 0 || isResetTimerCounter) timerCounter = (int)(duration / Time.fixedDeltaTime);
		isTimerDone = false;
	}

	public void StartTimer(float duration, int count = 1, bool resetTimerCounter = true)
	{
		startTime = DateTime.UtcNow;
		StartTimer(duration, resetTimerCounter);
		this.count = count;
	}

	public void StartTimer(float duration, DateTime startTime, int count = 1, bool resetTimerCounter = true)
	{
		this.startTime = startTime;
		StartTimer(duration, resetTimerCounter);
		this.count = count;
	}

	public void StopTimer()
	{
		isTimerActive = false;
		OnTimerReached = null;
		timerCounter = 0;
		duration = -1;
	}

	public void PauseTimer()
	{
		isTimerPaused = true;
	}

	public void ResumeTimer()
	{
		isTimerPaused = false;
	}

	public void TimerUpdate()
	{
		if (isTimerActive && timerMode == TimerMode.counterFixedUpdate && !isTimerPaused)
		{
			TimerFixedUpdate();

			if (GetRemainSeconds() == 0)
			{
				isTimerActive = false;
				isTimerDone = true;
				OnTimerReached?.Invoke();
				if (unsubscribeAllActions) OnTimerReached = null;
				if (loop) RestartTimer();
			}
			return;
		}

		if (isTimerActive && !isTimerPaused)
		{
			span = endTime - DateTime.UtcNow;

			if (GetRemainSeconds() == 0)
			{
				isTimerActive = false;
				isTimerDone = true;
				OnTimerReached?.Invoke();
			}
		}
	}

	private void TimerFixedUpdate()
	{
		if (timerCounter > 0)
		{
			timerCounter -= count;
			if (timerCounter < 0) timerCounter = 0;
			span = new TimeSpan(0, 0, 0, 0, (int)(timerCounter * Time.deltaTime * 1000));
		}
	}

	public float GetRemainSeconds()
	{
		float remain = 0;
		if (timerMode == TimerMode.dateTime) remain = Mathf.Ceil((float)span.TotalSeconds);
		if (timerMode == TimerMode.counterFixedUpdate) remain = timerCounter * Time.fixedDeltaTime;
		if (remain < 0) remain = 0;
		return remain;
	}

	public string GetRemainUsualTime(int parametersCount)
	{
		var h = span.Hours.ToString();
		if (span.Hours < 10) h = "0" + h;
		var m = span.Minutes.ToString();
		if (parametersCount == 2) m = (span.Minutes + span.Hours * 60).ToString();
		if (m.Length < 2) m = "0" + m;
		var s = span.Seconds.ToString();
		if (parametersCount == 1) s = (span.Seconds + span.Minutes * 60 + span.Hours * 60).ToString();
		if (s.Length < 2) s = "0" + s;
		var text = "";
		if (parametersCount == 3) text = h + ":" + m + ":" + s;
		if (parametersCount == 2) text = m + ":" + s;
		if (parametersCount == 1) text = s;
		return text;
	}

	public float GetTimeNormalized()
	{
		return (duration - (float)span.TotalSeconds) / duration;
	}

	public float GetRemainTimeNormalized()
	{
		return (float)span.TotalSeconds / duration;
	}

	public void RestartTimer(int count = 1)
	{
		if (duration == -1) return;
		timerCounter = 0;
		StartTimer(duration, count);
	}

	public void CancelTimer()
	{
		isTimerActive = false;
		isTimerDone = false;
	}

	public bool IsTimerPaused()
	{
		return isTimerPaused;
	}
}

public enum TimerMode
{
	dateTime, counterFixedUpdate
}