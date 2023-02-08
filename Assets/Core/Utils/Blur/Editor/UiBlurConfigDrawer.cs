using System;
using UnityEditor;
using UnityEngine;

namespace Core.Utils.Editor
{

    [CustomPropertyDrawer(typeof(UiBlurConfig))]
    public class UiBlurConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var method_prop = property.FindPropertyRelative("method");
            var method = (UiBlurMethod)method_prop.enumValueIndex;
            DrawPropertyField(ref position, method_prop);

            switch (method)
            {
                case UiBlurMethod.GaussianBlur:
                    var gaussian_blur_prop = property.FindPropertyRelative("gaussianBlurConfig");
                    
                    DrawPropertyField(ref position, gaussian_blur_prop.FindPropertyRelative("downSamplingTimes"));
                    DrawPropertyField(ref position, gaussian_blur_prop.FindPropertyRelative("spread"));
                    DrawPropertyField(ref position, gaussian_blur_prop.FindPropertyRelative("iterations"));
                    break;
                case UiBlurMethod.BoxBlur:
                    var box_blur_prop = property.FindPropertyRelative("boxBlurConfig");

                    DrawPropertyField(ref position, box_blur_prop.FindPropertyRelative("quality"));
                    DrawPropertyField(ref position, box_blur_prop.FindPropertyRelative("radius"));
                    DrawPropertyField(ref position, box_blur_prop.FindPropertyRelative("scaling"));
                    break;
                default: break;
            }

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var method_prop = property.FindPropertyRelative("method");
            var method = (UiBlurMethod)method_prop.enumValueIndex;

            var height = EditorGUI.GetPropertyHeight(method_prop);

            switch (method)
            {
                case UiBlurMethod.GaussianBlur:
                    var gaussian_blur_prop = property.FindPropertyRelative("gaussianBlurConfig");
                    
                    AddHeight(ref height, gaussian_blur_prop.FindPropertyRelative("downSamplingTimes"));
                    AddHeight(ref height, gaussian_blur_prop.FindPropertyRelative("spread"));
                    AddHeight(ref height, gaussian_blur_prop.FindPropertyRelative("iterations"));
                    break;
                case UiBlurMethod.BoxBlur:
                    var box_blur_prop = property.FindPropertyRelative("boxBlurConfig");

                    AddHeight(ref height, box_blur_prop.FindPropertyRelative("quality"));
                    AddHeight(ref height, box_blur_prop.FindPropertyRelative("radius"));
                    AddHeight(ref height, box_blur_prop.FindPropertyRelative("scaling"));
                    break;
                default: break;
            }

            return height;
        }

        public static void DrawPropertyField(ref Rect position, SerializedProperty property)
        {
            var height = EditorGUI.GetPropertyHeight(property);
            position.height = height;
            EditorGUI.PropertyField(position, property);
            position.y += height + 2;
        }

        public static void AddHeight(ref float height, SerializedProperty property)
        {
            height += EditorGUI.GetPropertyHeight(property) + 2;
        }
    }

}
