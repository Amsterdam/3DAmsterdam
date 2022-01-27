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
	public string IDColumnName = "";

    public int IDColumnIndex = 0;
    public int ColorColumnIndex = 0;

    public List<int> ColorColumnIndices = new List<int>();

	public void SetColorColumnIndex(string columnName)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            if(columnName == Columns[i])
            {
                ColorColumnIndex = i;
            }
        }
    }

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
            else if(IDColumnName == ""){
                IDColumnName = col;
                IDColumnIndex = i;
            }
        }
    }

    public Dictionary<string,Color> GetColorsAndIDs()
    {
        Dictionary<string, Color> idsAndColors = new Dictionary<string, Color>();

		for (int i = 1; i < Rows.Count; i++)
		{
            var cols = Rows[i];
            var id = cols[IDColumnIndex];
            var colorText = cols[ColorColumnIndex];

            Color color = Color.white;
            ColorUtility.TryParseHtmlString(colorText, out color);
            idsAndColors.Add(id, color);
        }

        return idsAndColors;
    }


    private bool IsColor(string content, out Color color)
    {
        return ColorUtility.TryParseHtmlString(content, out color);
	}
}

