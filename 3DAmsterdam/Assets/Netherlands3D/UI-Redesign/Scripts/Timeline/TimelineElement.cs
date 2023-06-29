using System;
using System.Collections.Generic;
using UnityEngine;

public class TimelineElement : ScriptableObject
{
    public string Name { get; private set; }
    public List<ColorLegendItem> Legend { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public TimelineElement(string name, List<ColorLegendItem> legend, DateTime startDate, DateTime endDate)
    {
        Name = name;
        Legend = legend;
        StartDate = startDate;
        EndDate = endDate;  
    }
}

public class ColorLegendItem : ScriptableObject
{
    public string Name { get; private set; }
    public Color Colour { get; private set; }

    public ColorLegendItem(string name, Color colour)
    {
        Name = name;
        Colour = colour;
    }

}
