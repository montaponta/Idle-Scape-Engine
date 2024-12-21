using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class AbstractUnit : MainRefs, IIDExtention
{
	public bool isEnable;
	public bool isNeedSave;
	public string id;
	public bool logUnitActionType;
	[NonSerialized] public UnitActionType unitActionType = UnitActionType.idler;
	[NonSerialized] public AbstractUnitAction unitAction;
	[NonSerialized] public Animator animator;
	[NonSerialized] public List<AbstractUnitAction> unitActionsList = new List<AbstractUnitAction>();
	public NavMeshAgent agent;
	[NonSerialized] public int health = -1;
	protected LineRenderer lineRenderer;
	[NonSerialized] public ProgressBarUI healthBar;
	[NonSerialized] public bool isApplyHit = true;
	[NonSerialized] public Transform target;
	public Action<int, int> OnHealthChanged;
	protected Dictionary<AbilityType, AbstractUnitAbility> abilitiesPair = new Dictionary<AbilityType, AbstractUnitAbility>();
	[NonSerialized] public Vector3 circlePoint;

	protected virtual void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
	}

	protected override void Start()
	{
		base.Start();
		//savingManager.unitsSavingData.GetUnitState(this);
		lineRenderer = GetComponentInChildren<LineRenderer>();
		if (health == -1) health = GetUnitData().GetHealth();
		DrawViewCircle(GetWatchingDistance());
		if (healthBar) healthBar.maxValue = GetMaxHealth();
	}

	protected virtual void Update()
	{
		foreach (var item in abilitiesPair) { item.Value.Update(); }

		if (healthBar)
		{
			AlignHealthBar();
			healthBar.SetValue(health);
		}

		if (!isEnable) return;

		if (GetRef<Main>().unitActionPermissionHandler.CanIAskPermission(this))
		{
			for (int i = 0; i < unitActionsList.Count; i++)
			{
				if (unitActionsList[i].CheckAction()) break;
			}
		}

		if (unitAction != null) unitAction.Update();
	}

	protected virtual void FixedUpdate()
	{
		foreach (var item in abilitiesPair) { item.Value.FixedUpdate(); }
		if (!isEnable) return;
		if (unitAction != null) unitAction.FixedUpdate();
	}

	protected virtual void AlignHealthBar()
	{
		var pos = Camera.main.WorldToScreenPoint(transform.position);
		healthBar.transform.position = pos + new Vector3(0, 100, 0) * GetRef<AbstractUI>().transform.localScale.y;
	}

	public float ObjectFinishTurning(Vector3 targetPos, float clampMin = -10, float clampMax = 10, bool rotate = true)
	{
		var targetLocalPos = transform.InverseTransformPoint(targetPos);
		var A = targetLocalPos.x;
		var B = targetLocalPos.z;
		var alpha = Mathf.Atan2(A, B) * Mathf.Rad2Deg;
		var angle = alpha;
		alpha = Mathf.Clamp(alpha, clampMin, clampMax);
		if (rotate) transform.Rotate(0, alpha, 0);
		return angle;
	}

	public void ObjectFinishTurning(Vector3 targetPos, float duration)
	{
		var targetLocalPos = transform.InverseTransformPoint(targetPos);
		var A = targetLocalPos.x;
		var B = targetLocalPos.z;
		var alpha = Mathf.Atan2(A, B) * Mathf.Rad2Deg;
		var angle = new Vector3(0, alpha, 0);
		transform.DORotate(transform.eulerAngles + angle, duration);
	}

	public virtual void DrawViewCircle(float radius)
	{
		if (lineRenderer == null) return;

		if (radius == 0)
		{
			lineRenderer.positionCount = 0;
			return;
		}

		List<Vector3> list = new List<Vector3>();
		float sectorAngle = 10;

		for (int i = 0; i <= 360 / sectorAngle; i++)
		{
			var angle = i * sectorAngle;
			var xPoint = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
			var yPoint = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
			list.Add(new Vector3(xPoint, yPoint, 0.2f));
		}

		lineRenderer.positionCount = list.Count;
		lineRenderer.SetPositions(list.ToArray());
	}

	public virtual void SetDamage(float damage, AbstractWeapon weapon)
	{
		health = (int)Mathf.Clamp(health - damage, 0, health);
		if (health <= 0) Die();
		OnHealthChanged?.Invoke(-(int)damage, health);
		//print($"{name}, {health}");
	}

	public virtual void Die()
	{
		SetActionTypeForced(UnitActionType.idler);
		isEnable = false;
		isApplyHit = false;
		if (healthBar) healthBar.SetValue(health);
		agent.enabled = false;
		if (lineRenderer) lineRenderer.positionCount = 0;
		DestroyUnit();
	}

	public virtual void Resurrect(int resurrectionIndex)
	{
		var oldHealth = health;
		health = (int)GetMaxHealth();
		agent.enabled = true;
		isEnable = true;
		isApplyHit = true;
		OnHealthChanged?.Invoke(health - oldHealth, health);
	}

	public void SetActionTypeForced(UnitActionType type)
	{
		var needAction = unitActionsList.Find(a => a.GetActionType() == type);
		if (needAction != null) needAction.StartAction();
		else
		{
			if (unitAction != null)
			{
				unitAction.OnFinish();
				unitAction.OnFinish(type);
				unitAction = null;
			}

			unitActionType = type;
			unitActionsList[0].LogUnitAction(type);
		}
	}

	public AbstractUnitAction GetUnitAction(UnitActionType type)
	{
		var v = unitActionsList.Find(a => a.GetActionType() == type);
		return v;
	}

	public virtual void SetAnimationPhase(int value)
	{
		foreach (var item in abilitiesPair.Values)
		{
			item.SetAnimationPhase(value);
		}
	}

	public virtual IEnumerator MoveToTargetCoroutine(Vector3 pos, Action OnReachedDestination = null, float delayTime = 0)
	{
		yield return new WaitForSeconds(delayTime);
		if (!agent.enabled) yield break;
		agent.isStopped = false;
		agent.SetDestination(pos);
		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => !agent.pathPending);
		yield return new WaitUntil(() => !agent.enabled || agent.remainingDistance <= agent.stoppingDistance + 0.1f);
		if (agent.enabled)
		{
			agent.isStopped = true;
			OnReachedDestination?.Invoke();
		}
	}

	public virtual IEnumerator MoveToTargetCoroutine(Transform target, Action OnReachedDestination = null, float delayTime = 0)
	{
		yield return new WaitForSeconds(delayTime);
		if (!agent.enabled) yield break;
		agent.isStopped = false;
		agent.SetDestination(target.position);
		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => !agent.pathPending);
		yield return new WaitUntil(() => !agent.enabled || agent.remainingDistance <= agent.stoppingDistance + 0.1f);
		if (agent.enabled)
		{
			agent.isStopped = true;
			OnReachedDestination?.Invoke();
		}
	}

	public virtual Coroutine MoveToTarget(Vector3 pos, Action OnReachedDestination = null, float delayTime = 0)
	{
		var coroutine = StartCoroutine(MoveToTargetCoroutine(pos, OnReachedDestination, delayTime));
		return coroutine;
	}

	public virtual Coroutine MoveToTarget(Transform target, Action OnReachedDestination = null, float delayTime = 0)
	{
		var coroutine = StartCoroutine(MoveToTargetCoroutine(target, OnReachedDestination, delayTime));
		return coroutine;
	}

	public virtual float GetWatchingDistance() { return 10; }
	public abstract IUnitData GetUnitData();

	public T GetSOData<T>() where T : IUnitData
	{
		return (T)GetUnitData();
	}
	public virtual void PlayAnimation(string animation, bool isPlay) { animator.SetBool(animation, isPlay); }
	protected virtual void DestroyUnit()
	{
		if (healthBar) Destroy(healthBar.gameObject);
		Destroy(gameObject);
	}
	public abstract float GetMaxHealth();
	public virtual AbstractUnitAbility GetUnitAbility(AbilityType type) { return abilitiesPair[type]; }
	public virtual T GetUnitAbility<T>(AbilityType type) where T : AbstractUnitAbility { return (T)abilitiesPair[type]; }
	public virtual AbstractUnitAbility GetUnitAbility<T>()
	{
		var ability = abilitiesPair.Where(a => a.Value is T).FirstOrDefault().Value;
		return ability;
	}

	public virtual void SetID(string id)
	{
		this.id = id;
	}

	public virtual string GetID()
	{
		return id;
	}

	public GameObject GetGameObject()
	{
		return gameObject;
	}

	public UnityEngine.Object GetObject()
	{
		return this;
	}

	public void ClearUnitActions()
	{
		foreach (var item in unitActionsList) { item.Dispose(); }
		unitActionsList.Clear();
	}

	private void OnDestroy()
	{
		ClearUnitActions();
	}
}
