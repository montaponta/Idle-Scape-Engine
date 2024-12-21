using DG.Tweening;
using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Main : MainRefs
{
	public static Main shared;
	public Camera mainCamera;
	public Action<int> OnSceneLevelComlete;
	public Action OnCameraEndAlign;
	public DebugTools debugTools;
	public float speedMultiplayer = 1;
	public UnitActionPermissionHandler unitActionPermissionHandler;
	private (float speed, Vector3 pos, bool leanCameraInclude) alignTargetData;
	private Vector3 camPos;

	private AbstractSavingManager savingManager => GetRef<AbstractSavingManager>();

	private void Awake()
	{
		shared = this;
		FindObjectOfType<Initializer>().AddReadySharedObject(this);
#if UNITY_EDITOR
		var arr = FindObjectsOfType<Main>();
		if (arr.Length > 1) Debug.LogError("Singleton already exist!!!");
#endif
	}

	public override void OnInitializeFinished()
	{
		if (!debugTools.isDebugActive)
		{
			savingManager.Init();
			savingManager.LoadData();
		}
		else
		{
			savingManager.Init();
			savingManager.dontSave = true;
			savingManager.isSavingDataLoadComplete = true;
			debugTools.SetDebugData();
		}
	}

	private void Update()
	{
		if (alignTargetData.speed > 0)
		{
			AlignCameraOnTargetLerp(mainCamera.transform, alignTargetData.pos, alignTargetData.speed * Time.deltaTime);
			if (Vector3.Distance(mainCamera.transform.position, camPos) <= 0.01f) OnEndAlignCamera();
			else camPos = mainCamera.transform.position;
		}
	}

	public void AlignCameraOnTargetLerp(Transform camTr, Vector3 targetPos, float lerpTime)
	{
		var pos = camTr.position;
		var camAngle = camTr.eulerAngles;
		camAngle.x = 90 - camAngle.x;
		camAngle.y = 360 - camAngle.y;
		var radianAngle = camAngle * Mathf.Deg2Rad;
		var r = Vector3.Distance(pos, targetPos);
		pos.z = targetPos.z - r * Mathf.Sin(radianAngle.x) * Mathf.Cos(radianAngle.y);
		pos.x = targetPos.x + r * Mathf.Sin(radianAngle.x) * Mathf.Sin(radianAngle.y);
		camTr.position = Vector3.Lerp(camTr.position, pos, lerpTime);
	}

	public void AlignCameraOnTarget(Vector3 targetPos, bool leanCameraInclude = true, bool onEndActionEnable = true, float speed = 10)
	{
		if (leanCameraInclude)
		{
			if (mainCamera.GetComponent<LeanDragCamera>())
				mainCamera.GetComponent<LeanDragCamera>().enabled = false;
			if (mainCamera.GetComponent<LeanPinchCamera>())
				mainCamera.GetComponent<LeanPinchCamera>().enabled = false;
		}

		alignTargetData = (speed, targetPos, leanCameraInclude);
	}

	private void OnEndAlignCamera()
	{
		if (alignTargetData.leanCameraInclude)
		{
			if (mainCamera.GetComponent<LeanDragCamera>())
				mainCamera.GetComponent<LeanDragCamera>().enabled = true;
			if (mainCamera.GetComponent<LeanPinchCamera>())
				mainCamera.GetComponent<LeanPinchCamera>().enabled = true;
		}

		OnCameraEndAlign?.Invoke();
		OnCameraEndAlign = null;
		alignTargetData = (0, Vector3.zero, false);
	}

	public IEnumerator ActionCoroutine(Action action, float delay = 0.1f)
	{
		yield return new WaitForSeconds(delay);
		action?.Invoke();
	}

	public IEnumerator ActionCoroutine(Action action, Func<bool> func)
	{
		yield return new WaitUntil(func);
		action?.Invoke();
	}

	public IEnumerator ActionCoroutine(List<(Action action, float delayBetweenActions)> actions, float delay = 0.1f)
	{
		yield return new WaitForSeconds(delay);

		foreach (var item in actions)
		{
			item.action.Invoke();
			yield return new WaitForSeconds(item.delayBetweenActions);
		}
	}

	public int GetSceneIndex()
	{
		return ES3.Load("sceneIndex", 0);
	}

	public string GetSceneName()
	{
		return SceneManager.GetActiveScene().name;
	}

	public Vector3 GetPositionOverTerrain(Vector3 pos)
	{
		Ray ray = new Ray(pos + Vector3.up * 50, -Vector3.up);
		RaycastHit raycastHit;
		int layerMask = 1 << 12;
		Physics.Raycast(ray, out raycastHit, 100, layerMask);
		return raycastHit.point;
	}

	/// <summary>
	/// findflag: 0 - Equal to name , 1 - Contains name
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="obj"></param>
	/// <param name="name"></param>
	/// <param name="includeInactive"></param>
	/// <param name="findFlag"></param>
	/// <returns></returns>
	public T GetTransformChild<T>(Transform obj, string name, bool includeInactive = false, int findFlag = 0) where T : Component
	{
		var arr = obj.GetComponentsInChildren<T>(includeInactive);
		var component = default(T);
		if (findFlag == 0) component = arr.Where(a => a.name == name).FirstOrDefault();
		if (findFlag == 1) component = arr.Where(a => a.name.Contains(name)).FirstOrDefault();
		if (component != null) return component;
		return null;
	}

	public GameObject GetWorldGO()
	{
		return GameObject.Find("World");
	}

	public void EndLevel()
	{
		savingManager.dontSave = false;
		savingManager.SaveData(true);
		savingManager.dontSave = true;
		SceneManager.LoadSceneAsync("Loading Scene");
	}

	public Vector3 GetAgentCirclePosition(Transform target, float radius, Transform unitTr, float pointFindAccuracy = 1, float curveLength = 0.7f)
	{
		List<Vector3> pointsList = new List<Vector3>();
		var alpha = (180 * curveLength) / (Mathf.PI * radius);
		var angle = 0f;
		var x0 = target.position.x;
		var z0 = target.position.z;

		while (angle < 360)
		{
			var x = x0 + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
			var z = z0 + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
			var newPoint = new Vector3(x, target.position.y, z);
			if (!pointsList.Contains(newPoint)) pointsList.Add(newPoint);
			angle += alpha;
		}

		var navmeshHit = new NavMeshHit();
		pointsList = pointsList.FindAll(a => NavMesh.SamplePosition(a, out navmeshHit, pointFindAccuracy, NavMesh.AllAreas));
		var tempList = pointsList.Where(a => !shared.IsPointBehindObstacle(target.position, a, radius, target)).ToList();
		if (tempList.Any()) pointsList = tempList;
		List<Vector3> pointsList1 = new List<Vector3>(pointsList);
		List<AbstractUnit> otherUnitsList = FindObjectsOfType<AbstractUnit>().ToList();
		otherUnitsList = otherUnitsList.FindAll(a => a.target == target && a.transform != unitTr);

		foreach (var unit in otherUnitsList)
		{
			var unitPoint = unit.circlePoint;
			var toClose = pointsList1.Find(a => Vector3.Distance(a, unitPoint) < 0.15f);
			if (toClose != Vector3.zero) pointsList1.Remove(toClose);
		}

		if (pointsList1.Count == 0) pointsList1 = pointsList;

		float dist = 100000;
		Vector3 point = Vector3.zero;

		foreach (var item in pointsList1)
		{
			var localDist = Vector3.Distance(unitTr.position, item);
			if (localDist < dist)
			{
				dist = localDist;
				point = item;
			}
		}

		return point;
	}

	private bool IsPointBehindObstacle(Vector3 centerPoint, Vector3 circlePoint, float radius, Transform obj)
	{
		centerPoint.y += 1f;
		circlePoint.y += 1f;
		var dir = circlePoint - centerPoint;
		Ray ray = new Ray(centerPoint, dir);
		var dist = dir.magnitude + radius + 0.2f;
		var arr = Physics.RaycastAll(ray, dist);
		var list = arr.Where(a => a.collider.transform != obj);
		var b = list.Any();
		return b;
	}

	public void SwitchLeanTouch(bool b)
	{
		if (mainCamera == null) return;
		if (mainCamera.GetComponent<LeanDragCamera>())
			mainCamera.GetComponent<LeanDragCamera>().enabled = b;
		if (mainCamera.GetComponent<LeanPinchCamera>())
			mainCamera.GetComponent<LeanPinchCamera>().enabled = b;
	}

	public List<CollectablesItemCount> ConvertDataToCollectablesItemCountList(object data)
	{
		List<CollectablesItemCount> list = new List<CollectablesItemCount>();

		if (data is List<NeedResource>)
		{
			var dataList = (List<NeedResource>)data;
			foreach (var item in dataList) list.Add(item.collectablesItemCount);
		}

		return list;
	}

	public Coroutine Invoke(Action action, float delay)
	{
		var coroutine = StartCoroutine(ActionCoroutine(action, delay));
		return coroutine;
	}

	public Coroutine InvokeWaitUntil(Action action, Func<bool> func)
	{
		var coroutine = StartCoroutine(ActionCoroutine(action, func));
		return coroutine;
	}

	public Coroutine InvokeDelayedActions(List<(Action action, float delayBetweenActions)> actions, float delay = 0.1f)
	{
		var coroutine = StartCoroutine(ActionCoroutine(actions, delay));
		return coroutine;
	}

	private void OnDestroy()
	{
		EventBus.Dispose();
	}
}
