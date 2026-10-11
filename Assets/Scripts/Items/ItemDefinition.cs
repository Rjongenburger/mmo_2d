using UnityEngine;

namespace TopDown
{
    public enum ItemAction { None, Light }

    [CreateAssetMenu(menuName = "TopDown/Item", fileName = "Item")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] string displayName = "Item";
        [SerializeField] Sprite icon;
        [SerializeField] bool stackable;
        [SerializeField, TextArea] string examine = "";
        [Tooltip("What left-clicking the item in the inventory does.")]
        [SerializeField] ItemAction primaryAction = ItemAction.None;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public bool Stackable => stackable;
        public string Examine => examine;
        public ItemAction PrimaryAction => primaryAction;

        /// <summary>OSRS-style verb shown in menus, or null when the item has no primary action.</summary>
        public string PrimaryVerb => primaryAction switch
        {
            ItemAction.Light => "Light",
            _ => null,
        };

        public void Setup(string name, Sprite itemIcon, bool isStackable, string examineText, ItemAction action)
        {
            displayName = name;
            icon = itemIcon;
            stackable = isStackable;
            examine = examineText;
            primaryAction = action;
        }
    }
}
