using UnityEditor;
using UnityEngine;

namespace PlayerSystems.Attributes.Editor {
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var minMaxAttribute = (MinMaxSliderAttribute)attribute;
            var propertyType = property.propertyType;
            
            label.tooltip = minMaxAttribute.Min.ToString("F") + " to " + minMaxAttribute.Max.ToString("F");
            
            Rect controlRect = EditorGUI.PrefixLabel(position, label);
            Rect[] splitRects = SplitRect(controlRect, 3);
            
            if (propertyType == SerializedPropertyType.Vector2) {
                var vector2Value = property.vector2Value;
                var minValue = vector2Value.x;
                var maxValue = vector2Value.y;
                
                EditorGUI.BeginChangeCheck();

                minValue = EditorGUI.FloatField(splitRects[0], float.Parse(minValue.ToString("F")));
                maxValue = EditorGUI.FloatField(splitRects[2], float.Parse(maxValue.ToString("F")));
                
                EditorGUI.MinMaxSlider(splitRects[1], ref minValue, ref maxValue, minMaxAttribute.Min, minMaxAttribute.Max);
                
                if (minValue < minMaxAttribute.Min) {
                    minValue = minMaxAttribute.Min;
                }
                if (maxValue > minMaxAttribute.Max) {
                    maxValue = minMaxAttribute.Max;
                }
                
                vector2Value = new Vector2(minValue > maxValue ? maxValue : minValue, maxValue);
                
                if (EditorGUI.EndChangeCheck()) {
                    property.vector2Value = vector2Value;
                }
            }
            else if (propertyType == SerializedPropertyType.Vector2Int) {
                var vector2IntValue = property.vector2IntValue;
                float minValue = vector2IntValue.x;
                float maxValue = vector2IntValue.y;
                
                EditorGUI.BeginChangeCheck();
                
                minValue = EditorGUI.FloatField(splitRects[0], minValue);
                maxValue = EditorGUI.FloatField(splitRects[2], maxValue);
                
                EditorGUI.MinMaxSlider(splitRects[1], ref minValue, ref maxValue, minMaxAttribute.Min, minMaxAttribute.Max);
                
                if (minValue < minMaxAttribute.Min) {
                    minValue = minMaxAttribute.Min;
                }
                
                if (maxValue > minMaxAttribute.Max) {
                    maxValue = minMaxAttribute.Max;
                }
                
                vector2IntValue = new Vector2Int((int)minValue, (int)maxValue);
                
                if (EditorGUI.EndChangeCheck()) {
                    property.vector2IntValue = vector2IntValue;
                }
            }
            else {
                EditorGUI.LabelField(position, label.text, "Use only with Vector2");
            }
        }
        
        Rect[] SplitRect(Rect rectToSplit, int n) {
            Rect[] rects = new Rect[n];

            for(int i = 0; i < n; i++){
                rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
            }

            int padding = (int)rects[0].width - 40;
            int space = 5;

            rects[0].width -= padding + space;
            rects[2].width -= padding + space;

            rects[1].x -= padding;
            rects[1].width += padding * 2;

            rects[2].x += padding + space;
        

            return rects;
        }
    }
}