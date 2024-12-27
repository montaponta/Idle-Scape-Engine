using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MainRefs
{
	public SlicedFilledImage bar;
	[SomeTextInInspector("or", FontStyle.BoldAndItalic)]
	public Image barImage;
	public TMP_Text countText;
	public bool isUsePercent;
	public float maxValue = 100;
	public int roundTo = 0;
	private Timer timer;
	public float value { get; private set; }
	private AbstractUI Ui => GetRef<AbstractUI>();

	/// <summary>
	/// Set Timer to get timer.GetRemainTimeNormalized self
	/// </summary>
	/// <param name="timer"></param>
	/// <param name="destroyFlag">0 - don't destroy, 1 - destroy in IconGrid, 2 - destroy as GameObject</param>
	public void Init(Timer timer, int destroyFlag, Transform target)
	{
		this.timer = timer;

		switch (destroyFlag)
		{
			case 1:
				if (target) timer.OnTimerReached += () => Ui.DestroyIcon(target, gameObject);
				else throw new NullReferenceException();
				break;
			case 2:
				timer.OnTimerReached += () => Destroy(gameObject);
				break;
		}
	}

	public void Init(Timer timer, Action action)
	{
		this.timer = timer;
		timer.OnTimerReached += () => action?.Invoke();
	}

	public void SetValue(float value)
	{
		this.value = value;
		if (bar) bar.fillAmount = value / maxValue;
		if (barImage) barImage.fillAmount = value / maxValue;
		SetText(value);
	}

	private void SetText(float value)
	{
		if (!countText) return;
		value = (float)System.Math.Round(value, roundTo);

		if (isUsePercent)
		{
			if (maxValue != 100) value *= 100 / maxValue;
			countText.text = value.ToString();
			countText.text += "%";
		}
		else countText.text = value.ToString();
	}

	private void FixedUpdate()
	{
		if (timer != null) SetValue(timer.GetTimeNormalized());
	}
}
