using UnityEngine;

public abstract class ItemBase
{
    public ItemData ItemData { get; protected set; }
    public ItemBase(ItemData data)
    {
        ItemData = data;
    }

    public abstract void Use();
}
