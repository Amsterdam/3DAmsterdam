using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CsvColorsFinder : CsvContentFinder
{
	public string IDColumnName = "";

    public int IDColumnIndex = 0;
    public int ColorColumnIndex = 0;

    public List<int> IDColumnIndices = new List<int>();
    public List<int> ColorColumnIndices = new List<int>();

    public void SetIDColumn(string columnName)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            if (columnName == Columns[i])
            {
                IDColumnIndex = i;
            }
        }
    }

    public void SetColorColumn(string columnName)
    {
        for (int i = 0; i < Columns.Length; i++)
        {
            if(columnName == Columns[i])
            {
                ColorColumnIndex = i;
            }
        }
    }

    public CsvColorsFinder(string[] Columns, List<string[]> Rows)
    {
        this.Columns = Columns;
        this.Rows = Rows;

        FindColors();

        if (Columns.Length == 1)
        {
            Status = CsvContentFinderStatus.Failed;
            StatusMessageLines.Add("Geen kolommen gedetecteerd, controleer of de kolommen gescheiden zijn met het ; teken in het CSV bestand");
        }
        else if (ColorColumnIndices.Count < 1)
        {
            Status = CsvContentFinderStatus.Failed;
            StatusMessageLines.Add("Geen kolommen met kleuren gevonden. Zorg dat de kleurkolommen een HTML kleurcode hebben zoals: #FF0000");
        }
        else
        {
            Status = CsvContentFinderStatus.Success;
        }
    }

	void FindColors()
    {
        IDColumnName = "";

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
            else{
                IDColumnIndices.Add(i);
                if (IDColumnName == "")
                {
                    IDColumnName = col;
                    IDColumnIndex = i;
                }
            }
        }

        Status = (ColorColumnIndices.Count > 0 && IDColumnIndices.Count > 0) ? CsvContentFinderStatus.Success : CsvContentFinderStatus.Failed;
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
            if(!idsAndColors.ContainsKey(id))
                idsAndColors.Add(id, color);
        }

        return idsAndColors;
    }

    private bool IsColor(string content, out Color color)
    {
        Debug.Log("FIND COLOR " + content);
        bool isAColor = ColorUtility.TryParseHtmlString(content, out color);
        Debug.Log(content + " is Color " + isAColor);
        return isAColor;
	}
}

