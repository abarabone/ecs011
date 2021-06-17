using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class VisualElementEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var container = new VisualElement();
        var iterator = this.serializedObject.GetIterator();
        if (!iterator.NextVisible(true)) return container;
        while (iterator.NextVisible(false))
        {
            var propfield = new PropertyField(iterator.Copy())
            {
                name = "PropertyField:" + iterator.propertyPath,
            };
            if (iterator.propertyPath == "m_Script" && this.serializedObject.targetObject != null)
            {
                propfield.SetEnabled(false);
            }
            container.Add(propfield);
        }
        return container;
    }
}

//public static class PropertyDrawerUtility
//{
//    public static void DrawDefaultGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        property = property.serializedObject.FindProperty(property.propertyPath);
//        var fieldRect = position;
//        fieldRect.height = EditorGUIUtility.singleLineHeight;

//        using (new EditorGUI.PropertyScope(fieldRect, label, property))
//        {
//            if (property.hasChildren)
//            {
//                // �q�v�f������ΐ܂��ݕ\��
//                property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, label);
//            }
//            else
//            {
//                // �q�v�f��������΃��x�������\��
//                EditorGUI.LabelField(fieldRect, label);
//                return;
//            }
//            fieldRect.y += EditorGUIUtility.singleLineHeight;
//            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

//            if (property.isExpanded)
//            {

//                using (new EditorGUI.IndentLevelScope())
//                {
//                    // �ŏ��̗v�f��`��
//                    property.NextVisible(true);
//                    var depth = property.depth;
//                    EditorGUI.PropertyField(fieldRect, property, true);
//                    fieldRect.y += EditorGUI.GetPropertyHeight(property, true);
//                    fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

//                    // ����ȍ~�̗v�f��`��
//                    while (property.NextVisible(false))
//                    {

//                        // depth���ŏ��̗v�f�Ɠ������̂̂ݏ���
//                        if (property.depth != depth)
//                        {
//                            break;
//                        }
//                        EditorGUI.PropertyField(fieldRect, property, true);
//                        fieldRect.y += EditorGUI.GetPropertyHeight(property, true);
//                        fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
//                    }
//                }
//            }
//        }
//    }

//    public static float GetDefaultPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        property = property.serializedObject.FindProperty(property.propertyPath);
//        var height = 0.0f;

//        // �v���p�e�B��
//        height += EditorGUIUtility.singleLineHeight;
//        height += EditorGUIUtility.standardVerticalSpacing;

//        if (!property.hasChildren)
//        {
//            // �q�v�f��������΃��x�������\��
//            return height;
//        }

//        if (property.isExpanded)
//        {

//            // �ŏ��̗v�f
//            property.NextVisible(true);
//            var depth = property.depth;
//            height += EditorGUI.GetPropertyHeight(property, true);
//            height += EditorGUIUtility.standardVerticalSpacing;

//            // ����ȍ~�̗v�f
//            while (property.NextVisible(false))
//            {
//                // depth���ŏ��̗v�f�Ɠ������̂̂ݏ���
//                if (property.depth != depth)
//                {
//                    break;
//                }
//                height += EditorGUI.GetPropertyHeight(property, true);
//                height += EditorGUIUtility.standardVerticalSpacing;
//            }
//            // �Ō�̓X�y�[�X�s�v�Ȃ̂ō폜
//            height -= EditorGUIUtility.standardVerticalSpacing;
//        }

//        return height;
//    }
//}
