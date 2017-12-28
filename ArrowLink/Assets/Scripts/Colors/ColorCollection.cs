using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ColorCollection : ScriptableObject {

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

    public enum GrabbableColor
    {
        Back,
        Tile,
        TileShadow,
        LinkBase,
        LinkOn,
        LinkSuper,
        Slot,
        SlotShadow,
        ButtonLight,
        ButtonDark,
        TextLight,
        TextDark,
        LinkDark,
        DotSlot,
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
            default:
                throw new System.NotSupportedException();
        }
    }
}
