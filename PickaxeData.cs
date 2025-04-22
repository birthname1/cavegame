using UnityEngine;
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "PickaxeData", menuName = "Pickaxe/PickaxeData")]
public class PickaxeData : ScriptableObject
{
    public float size;
    public int identity;
    public Item item;

    public Rarity RarityAssignment() {
        if (size > 2.75f) {
            return Rarity.Legendary;
        }
        else if (size > 1.9f) {
            return Rarity.Epic;
        }
        else if (size > 1.5f) {
            return Rarity.Rare;
        }
        else if (size > 1.0f){
            return Rarity.Uncommon;
        }
        else return Rarity.Common;
    }
    
    public Color GetColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Color.white;
            case Rarity.Uncommon:
                return Color.green;
            case Rarity.Rare:
                return Color.cyan;
            case Rarity.Epic:
                return new Color(0.6f, 0f, 0.8f); // purple
            case Rarity.Legendary:
                return new Color(1f, 0.5f, 0f); // orange
            default:
                return Color.gray;
        }
    }
    
}

