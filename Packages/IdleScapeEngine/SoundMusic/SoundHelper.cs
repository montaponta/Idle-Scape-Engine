using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SoundHelper : MainRefs, IPointerClickHandler
{
    public SoundType soundType;

    public void OnPointerClick(PointerEventData eventData)
    {
        var btn = GetComponent<Button>();
        if (btn && !btn.interactable) return;
        GetRef<SoundMusic>().PlaySound(soundType);
    }
}
