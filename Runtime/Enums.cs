public class Enums { }

public enum ResourceType
{
    none = 0,
    wood = 1,
    stone = 2,
    axe = 3,
    pickaxe = 4,
    tool = 5,
    backpack = 6,
    workbench = 7,
    unknown = 8,
    storage = 9
}

public enum UnitActionType
{
    none = 0,
    idler = 1,
    checkCollectSomething = 2,
    checkUnloadBackpack = 3,
    checkCraftSomething = 4,
    collectForCollectingTask = 6,
    checkCollectingTask = 7,
    checkQueueUnitPlace = 8,
    checkOpenContainer = 11,
    readyForOpenContainer = 12,
    readyForCraft = 13,
    readyForUnloadBackpack = 14,
    readyToCollectSomething = 15,
    readyForCollectingTask = 16,
}

public enum ResourceProducerType
{
    none, onScene, loot, drop
}

public enum AbilityType
{
    none,
    movement,
    collect,
    openContainer,
}

public enum CollectAnimationType
{
    none = 0,
    PickUp = 1,
    Chop = 2,
    StoneKick = 3,
}

public enum ResourceAnimationType
{
    none, scale, treeVibration
}

public enum PrefabType
{
    none, craftBar, panelContainer, openResourceProducerPanel, openContainerResourcePanel,
    needResourcePanel, resourceCountPanel, resourceProducerPanel, ñharacter
}
