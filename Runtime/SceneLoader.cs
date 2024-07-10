using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public List<string> scenesList;
    public ProgressBarUI progressBarUI;
    public Transform barIconTr;
    public Vector2 rangeXPosBarIcon;
    private float progress, barProgress;
    private AsyncOperation operation;
    private int sceneIndex;
    private bool canLoad;

    private void Start()
    {
        var key = ES3.Load("customSavingDataKey", AbstractSavingManager.GetCustomSavingDataPath(), "");
        if (key == "") sceneIndex = ES3.Load("sceneIndex", 0);
        else sceneIndex = ES3.Load("sceneIndex", $"SaveFile_{key}.es3", 0);
        if (barIconTr)
            barIconTr.localPosition = new Vector3(rangeXPosBarIcon.x, barIconTr.localPosition.y, 0);
        StartCoroutine(AsyncLoading());
    }

    private void Update()
    {
        canLoad = true;
        if (operation != null) progress = operation.progress / 0.9f;

        if (barProgress < progress)
        {
            barProgress += Time.deltaTime;
            barProgress = Mathf.Clamp(barProgress, 0, 1);
            progressBarUI.SetValue(barProgress * 100);

            if (barIconTr)
            {
                var pixelsPerUnit = (rangeXPosBarIcon.y - rangeXPosBarIcon.x) / 100;
                barIconTr.localPosition = new Vector3(rangeXPosBarIcon.x + pixelsPerUnit * barProgress * 100, barIconTr.localPosition.y, 0);
            }

            if (barProgress == 1) operation.allowSceneActivation = true;
        }
    }

    private IEnumerator AsyncLoading()
    {
        yield return new WaitUntil(() => canLoad);
        operation = SceneManager.LoadSceneAsync(scenesList[sceneIndex]);
        operation.allowSceneActivation = false;
    }
}
