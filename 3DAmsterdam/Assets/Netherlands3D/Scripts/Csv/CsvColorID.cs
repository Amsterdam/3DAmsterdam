using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CsvColorID : CsvContentFinder
{
	public string IDColumnName;
    public List<int> ColorColumnIndices = new List<int>();
	public string ColorColumnName { get; internal set; }

    public CsvColorID(string[] Columns, List<string[]> Rows)
    {
        this.Columns = Columns;
        this.Rows = Rows;

        FindColors();
    }

	void FindColors()
    {
        //finding colors
        var row1 = Rows.First();
        Color color;
        for (int i = 0; i < Columns.Length; i++)
        {
            var col = row1[i];
            if (IsColor(col, out color))
            {
                ColorColumnIndices.Add(i);
            }
        }
    }

    private bool IsColor(string content, out Color color)
    {
        return ColorUtility.TryParseHtmlString(content, out color);
	}
}

