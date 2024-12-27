using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
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


	public void Init(Timer timer, bool destroyOnReached = true)
	{
		this.timer = timer;
		if (destroyOnReached)
			timer.OnTimerReached += () => Destroy(gameObject);
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
		if (timer != null) SetValue(timer.GetRemainTimeNormalized());
	}
}
