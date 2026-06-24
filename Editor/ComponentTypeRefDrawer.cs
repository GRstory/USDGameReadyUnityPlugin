using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using USDGameReady;

namespace USDGameReady.Editor
{
    [CustomPropertyDrawer(typeof(ComponentTypeRef))]
    public class ComponentTypeRefDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var typeNameProp = property.FindPropertyRelative(nameof(ComponentTypeRef.typeName));

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            var label = new Label(ResolveLabel(property));
            label.style.minWidth = 150;
            label.style.flexShrink = 0;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(label);

            var button = new Button();
            button.text = FormatButtonLabel(typeNameProp.stringValue);
            button.style.flexGrow = 1;

            button.clicked += () =>
            {
                var dropdown = new ComponentTypeDropdown(new AdvancedDropdownState(), selected =>
                {
                    typeNameProp.stringValue = selected ?? string.Empty;
                    typeNameProp.serializedObject.ApplyModifiedProperties();
                    button.text = FormatButtonLabel(typeNameProp.stringValue);
                });
                dropdown.Show(button.worldBound);
            };

            row.Add(button);
            return row;
        }

        static string ResolveLabel(SerializedProperty property)
        {
            // propertyPath ends with ".value" (the ImportSetting<T>.value field).
            // Sibling field "id" is on the same ImportSetting<T>.
            var path = property.propertyPath;
            var cut = path.LastIndexOf(".value", StringComparison.Ordinal);
            if (cut > 0)
            {
                var parentPath = path.Substring(0, cut);
                var idProp = property.serializedObject.FindProperty(parentPath + ".id");
                if (idProp != null && !string.IsNullOrEmpty(idProp.stringValue))
                    return idProp.stringValue;
            }
            return ObjectNames.NicifyVariableName(property.name);
        }

        static string FormatButtonLabel(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return "(Default)";
            var t = Type.GetType(typeName);
            return t != null ? t.FullName : $"<Missing> {typeName}";
        }
    }

    class ComponentTypeDropdown : AdvancedDropdown
    {
        readonly Action<string> m_OnSelected;

        public ComponentTypeDropdown(AdvancedDropdownState state, Action<string> onSelected) : base(state)
        {
            m_OnSelected = onSelected;
            minimumSize = new Vector2(300, 400);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Component");
            root.AddChild(new TypeItem("(Default)", null));
            root.AddSeparator();

            var types = new List<Type>
            {
                typeof(UnityEngine.AI.NavMeshAgent),
                typeof(CharacterController),
            };

            foreach (var t in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
            {
                if (t.IsAbstract || t.IsGenericTypeDefinition) continue;
                if (IsUserAssembly(t.Assembly))
                    types.Add(t);
            }

            foreach (var t in types.Distinct().OrderBy(t => t.FullName))
                root.AddChild(new TypeItem(t.FullName, t.AssemblyQualifiedName));

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeItem ti)
                m_OnSelected?.Invoke(ti.typeName);
        }

        static bool IsUserAssembly(Assembly a)
        {
            var name = a.GetName().Name;
            if (string.IsNullOrEmpty(name)) return false;
            string[] excluded = { "UnityEngine", "UnityEditor", "Unity.", "com.unity",
                                  "System", "mscorlib", "netstandard", "Microsoft",
                                  "Mono.", "nunit.", "JetBrains." };
            return !excluded.Any(p => name.StartsWith(p, StringComparison.Ordinal));
        }

        class TypeItem : AdvancedDropdownItem
        {
            public readonly string typeName;
            public TypeItem(string label, string tn) : base(label) { typeName = tn; }
        }
    }
}
