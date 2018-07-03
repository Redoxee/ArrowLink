using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ColorCollection : ScriptableObject {
    public Sprite CollectionIcon;


    public Color Back;

    public Color Tile;
    public Color TileShadow;
    public Color LinkBase;
    public Color LinkOn;
    public Color LinkSuper;

    public Color LinkDark;

    public Color Slot;
    public Color SlotShadow;

    public Color DotSlot;

    public Color ButtonLight;
    public Color ButtonDark;
    public Color TextLight;
    public Color TextDark;

    public Color Icon;
    public Color TextOnBack;

    public StatusBarStyle StatBarStyle = StatusBarStyle.Light;

    public enum StatusBarStyle
    {
        Light = 1,
        Dark = 2,
    }

    public enum GrabbableColor
    {
        Back = 0,
        Tile = 1,
        TileShadow = 2,
        LinkBase = 3,
        LinkOn = 4,
        LinkSuper = 5,
        Slot = 6,
        SlotShadow = 7,
        ButtonLight = 8,
        ButtonDark = 9,
        TextLight = 10,
        TextDark = 11,
        LinkDark = 12,
        DotSlot = 13,
        ButtonIcon = 14,
        TextOnBack = 15,
    }

    public Color GetColor(GrabbableColor c)
    {
        switch (c)
        {
            case GrabbableColor.Back:
                return Back;
            case GrabbableColor.Tile:
                return Tile;
            case GrabbableColor.TileShadow:
                return TileShadow;
            case GrabbableColor.LinkBase:
                return LinkBase;
            case GrabbableColor.LinkOn:
                return LinkOn;
            case GrabbableColor.LinkSuper:
                return LinkSuper;
            case GrabbableColor.Slot:
                return Slot;
            case GrabbableColor.SlotShadow:
                return SlotShadow;
            case GrabbableColor.ButtonLight:
                return ButtonLight;
            case GrabbableColor.ButtonDark:
                return ButtonDark;
            case GrabbableColor.TextLight:
                return TextLight;
            case GrabbableColor.TextDark:
                return TextDark;
            case GrabbableColor.LinkDark:
                return LinkDark;
            case GrabbableColor.DotSlot:
                return DotSlot;
            case GrabbableColor.ButtonIcon:
                return Icon;
            case GrabbableColor.TextOnBack:
                return TextOnBack;
            default:
                throw new System.NotSupportedException();
        }
    }
}
