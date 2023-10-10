using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinecraftModel : ScriptableObject
{
    public ResourceLocation ID;

    public virtual void FixNamespaces() { }
    public abstract bool ExportToJson(QuickJSONBuilder builder);
    public abstract bool ExportInventoryVariantToJson(QuickJSONBuilder builder);
}
