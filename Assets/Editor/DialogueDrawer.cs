using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

// 对话选项的自定义编辑器
/*
[CustomPropertyDrawer(typeof(OptionEffect),true)]
public class DialogueDrawer : PropertyDrawer {
   
    private const float PADDING = 2f;
    private const float BUTTON_WIDTH = 20f;
    
    private string[] _subTypeNames;
    private Type[] _subTypes;
    private bool _isExpanded;
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!_isExpanded)
            return EditorGUIUtility.singleLineHeight;
            
        float height = EditorGUIUtility.singleLineHeight * 2 + PADDING;
        
        // 获取子类的所有字段高度
        SerializedProperty dataProp = property.FindPropertyRelative("line");
        while (dataProp.NextVisible(true))
        {
            height += EditorGUI.GetPropertyHeight(dataProp, true) + PADDING;
        }
        
        return height;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InitializeSubTypes();
        
        Rect mainRect = position;
        mainRect.height = EditorGUIUtility.singleLineHeight;
        
        // 绘制类型选择下拉菜单
        Rect typeRect = mainRect;
        typeRect.width -= BUTTON_WIDTH + PADDING;
        
        string currentTypeName = GetCurrentTypeName(property);
        int selectedIndex = Array.IndexOf(_subTypeNames, currentTypeName);
        if (selectedIndex == -1) selectedIndex = 0;
        
        EditorGUI.BeginProperty(position, label, property);
        
        // 显示类型选择器
        EditorGUI.BeginChangeCheck();
        selectedIndex = EditorGUI.Popup(typeRect, label.text, selectedIndex, _subTypeNames);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeEffectType(property, _subTypes[selectedIndex]);
        }
        
        // 展开/折叠按钮
        Rect foldoutRect = mainRect;
        foldoutRect.x = typeRect.xMax + PADDING;
        foldoutRect.width = BUTTON_WIDTH;
        
        _isExpanded = EditorGUI.Foldout(foldoutRect, _isExpanded, GUIContent.none, true);
        // 如果展开，绘制子类的字段
        if (_isExpanded)
        {
            Debug.Log($"[Editor] 当前选择的效果类型: {currentTypeName}");
            Rect dataRect = position;
            dataRect.y += EditorGUIUtility.singleLineHeight + PADDING;
            dataRect.height = EditorGUIUtility.singleLineHeight;
            
            // 绘制effectName字段
            SerializedProperty nameProp = property.FindPropertyRelative("type");
            EditorGUI.PropertyField(dataRect, nameProp);
            dataRect.y += EditorGUI.GetPropertyHeight(nameProp, true) + PADDING;
            
            // 绘制子类的其他字段
            SerializedProperty dataProp = nameProp.Copy();
            bool hasNext = dataProp.NextVisible(true);
            while (hasNext)
            {
                SerializedProperty endProp = property.GetEndProperty();
                if (SerializedProperty.EqualContents(dataProp, endProp))
                    break;
                    
                EditorGUI.PropertyField(dataRect, dataProp, true);
                dataRect.y += EditorGUI.GetPropertyHeight(dataProp, true) + PADDING;
                hasNext = dataProp.NextVisible(false);
            }
        }
        
        
        EditorGUI.EndProperty();
    }
    
    private void InitializeSubTypes()
    {
        
        if (_subTypes != null) return;
        
        // 获取OptionEffect的所有非抽象子类
        List<Type> subTypeList = new List<Type>();
        List<string> subTypeNameList = new List<string>();
        
        Assembly assembly = Assembly.GetAssembly(typeof(OptionEffect));
        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(OptionEffect)) && !type.IsAbstract)
            {
                subTypeList.Add(type);
                subTypeNameList.Add(type.Name);
            }
        }
        
        _subTypes = subTypeList.ToArray();
        _subTypeNames = subTypeNameList.ToArray();
    }
    
    private string GetCurrentTypeName(SerializedProperty property)
    {
        
        // 获取当前序列化对象的类型名称
        string typeName = property.name;
            // 处理类型名称

        if (string.IsNullOrEmpty(typeName))
            return _subTypeNames.Length > 0 ? _subTypeNames[0] : "None";
            
        // 解析类型名称（格式："Assembly-CSharp, Namespace.TypeName"）
        string[] parts = typeName.Split(',');
        if (parts.Length >= 2)
        {
            return parts[1].Trim();
        }
        
        return "Unknown";
    }
    
    private void ChangeEffectType(SerializedProperty property, Type newType)
    {
        // 更改序列化对象的类型
        object newInstance = Activator.CreateInstance(newType);
        SerializedObject newSerializedObject = new((UnityEngine.Object)newInstance);
        newSerializedObject.Update();
        // 复制新实例的属性值到原属性
        CopySerializedProperties(newSerializedObject, property.serializedObject, newType);
        property.serializedObject.ApplyModifiedProperties();
    }

    // 辅助方法: 复制一个序列化对象的属性到另一个
    private void CopySerializedProperties(SerializedObject source, SerializedObject destination, Type type)
    {
        SerializedProperty sourceProp = source.GetIterator();
        bool enterChildren = true;
        
        while (sourceProp.NextVisible(enterChildren))
        {
            // 在目标对象中查找相同名称的属性
            SerializedProperty destProp = destination.FindProperty(sourceProp.name);
            
            if (destProp != null && AreTypesCompatible(sourceProp, destProp))
            {
                // 复制属性值
                CopyPropertyValue(sourceProp, destProp);
            }
            
            enterChildren = false;
        }
    }
    // 辅助方法: 检查两个属性类型是否兼容
    private bool AreTypesCompatible(SerializedProperty source, SerializedProperty dest)
    {
        return source.propertyType == dest.propertyType;
        // 可以添加更复杂的类型兼容性检查
    }

    // 辅助方法: 复制属性值
    private void CopyPropertyValue(SerializedProperty source, SerializedProperty dest)
    {
        // 处理不同类型的属性复制
        switch (source.propertyType)
        {
            case SerializedPropertyType.Integer:
                dest.intValue = source.intValue;
                break;
            case SerializedPropertyType.Float:
                dest.floatValue = source.floatValue;
                break;
            case SerializedPropertyType.Boolean:
                dest.boolValue = source.boolValue;
                break;
            // 添加更多类型...
            default:
                // 对于复杂类型，递归复制
                if (source.hasVisibleChildren)
                {
                    SerializedProperty sourceChild = source.Copy();
                    SerializedProperty destChild = dest.Copy();
                    
                    sourceChild.Next(true);
                    destChild.Next(true);
                    
                    int depth = sourceChild.depth;
                    
                    while (sourceChild.depth >= depth && destChild.depth >= depth)
                    {
                        if (sourceChild.depth == depth && AreTypesCompatible(sourceChild, destChild))
                        {
                            CopyPropertyValue(sourceChild, destChild);
                        }
                        
                        if (!sourceChild.Next(false) || !destChild.Next(false))
                            break;
                    }
                }
                break;
        }
    }
}
*/