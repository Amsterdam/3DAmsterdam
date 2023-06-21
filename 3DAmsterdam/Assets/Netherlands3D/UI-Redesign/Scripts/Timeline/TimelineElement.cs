using System;
using System.Collections.Generic;
using UnityEngine;

public class TimelineElement : ScriptableObject
{
    public string Name { get; private set; }
    public List<ColorLegendItem> Legend { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
}

public class ColorLegendItem : ScriptableObject
{
    public string Name { get; private set; }
    public Color Colour { get; private set; }

}
