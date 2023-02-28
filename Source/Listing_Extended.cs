using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace BetterPacifists;

public class Listing_Extended : Listing_Standard
{
    public Listing_Extended(GameFont font) : base(font)
    {
    }

    public Listing_Extended()
    {
    }

    public Listing_Extended(Rect boundingRect, Func<Vector2> boundingScrollPositionGetter) : base(boundingRect, boundingScrollPositionGetter)
    {
    }

    public void CheckboxLabeledIcon(string label, ref bool checkOn, ThingDef def, ThingStyleDef style = null, float height = 0f)
    {
        var fullRect = GetRect(
            height == 0f
                ? Text.CalcHeight(label, ColumnWidth * 0.7f)
                : height);

        if (BoundingRectCached != null && !fullRect.Overlaps(BoundingRectCached.Value))
            return;

        if (def != null)
            Widgets.ThingIcon(fullRect.LeftPart(0.25f), def, GenStuff.DefaultStuffFor(def), style);
        Widgets.CheckboxLabeled(fullRect.RightPart(0.7f), label, ref checkOn);

        Gap(verticalSpacing);
    }
}