using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbstractTabsUI : MainRefs
{
	public List<GameObject> panelsList;
	public List<Transform> panelButtonsList;
	public Color activeColor, unactiveColor;
	public Sprite activeSprite, unactiveSprite;

	protected override void Start()
	{
		base.Start();
		OnClickTabBtn(0);
	}

    public virtual void OnClickTabBtn(int btnIndex)
	{
		panelsList.ForEach(a => a.SetActive(false));
		panelsList[btnIndex].SetActive(true);

		if (activeColor != Color.clear && unactiveColor != Color.clear)
		{
			panelButtonsList.ForEach(a => a.GetComponent<Image>().color = unactiveColor);
			panelButtonsList[btnIndex].GetComponent<Image>().color = activeColor;
		}

		if (activeSprite != null && unactiveSprite != null)
		{
			panelButtonsList.ForEach(a => a.GetComponent<Image>().sprite = unactiveSprite);
			panelButtonsList[btnIndex].GetComponent<Image>().sprite = activeSprite;
		}
	}

	public void OnClickCloseBtn()
	{
		GetRef<AbstractUI>().ShowHidePanel(gameObject);
	}
}
