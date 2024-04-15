using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
	public List<string> scenesList;
	public ProgressBarUI progressBarUI;
	public Transform planeTr;
	public Vector2 rangeXPosPlane;
	private float progress;
	private AsyncOperation operation;
	private int sceneIndex;

	private void Start()
	{
		sceneIndex = ES3.Load("sceneIndex", 0);
		planeTr.localPosition = new Vector3(rangeXPosPlane.x, planeTr.localPosition.y, 0);
		StartCoroutine(AsyncLoadnig());
		DontDestroyOnLoad(gameObject);
	}

	private IEnumerator AsyncLoadnig()
	{
		operation = SceneManager.LoadSceneAsync(scenesList[sceneIndex]);
		//operation.allowSceneActivation = false;

		while (!operation.isDone)
		{
			progress = operation.progress / 0.9f;
			progressBarUI.SetValue(progress * 100);
			var pixelsPerUnit = (rangeXPosPlane.y - rangeXPosPlane.x) / 100;
			planeTr.localPosition = new Vector3(rangeXPosPlane.x + pixelsPerUnit * progress * 100, planeTr.localPosition.y, 0);
			yield return null;
		}
	}
}
