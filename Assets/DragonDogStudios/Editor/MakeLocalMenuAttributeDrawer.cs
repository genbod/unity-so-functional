using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[OdinDrawer]
public class MakeLocalMenuAttributeDrawer<T> : OdinAttributeDrawer<MakeLocalMenuAttribute, T>, IDefinesGenericMenuItems
    where T : ScriptableObject
{
    protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, MakeLocalMenuAttribute attribute, GUIContent label)
    {
        this.CallNextDrawer(entry.Property, label);
    }

    public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
    {
        if (genericMenu.GetItemCount() > 0)
        {
            genericMenu.AddSeparator("");
        }

        genericMenu.AddItem(new GUIContent("Create Local"), false, () => this.CreateLocalScriptableValue(property));
    }

    private void CreateLocalScriptableValue(InspectorProperty property)
    {
        var parent = property.Tree.UnitySerializedObject.targetObject;
        // Check for existing asset
        var assetPath = AssetDatabase.GetAssetPath(parent);
        var entry = (IPropertyValueEntry<T>)property.ValueEntry;
        if (entry.SmartValue != null && AssetDatabase.GetAssetPath(entry.SmartValue) == assetPath)
        {
            Object.DestroyImmediate(entry.SmartValue, true);
        }
        var typeOfValue = entry.TypeOfValue;
        var newObject = ScriptableObject.CreateInstance<T>();
        newObject.name = entry.Property.NiceName;
        AssetDatabase.AddObjectToAsset(newObject, parent);
        AssetDatabase.SaveAssets();
        entry.SmartValue = newObject;
        entry.ApplyChanges();
    }
}
