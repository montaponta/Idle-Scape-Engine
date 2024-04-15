

using System.Collections.Generic;

public interface IGeneralFunctionalWindow
{
    public IGeneralFunctionalWindow OpenWindow(string windowName);
    public bool CanShowElement(string elementName);
    public void SetParameters(AbstractQuestBlockPart blockPart, object obj);
    public Quests GetMainScript();
    public List<QuestData> GetQuestDatasList();
    public int GetIndexByTag(string tag);
}
