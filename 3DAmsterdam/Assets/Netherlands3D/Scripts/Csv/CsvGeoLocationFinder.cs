using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CsvGeoLocationFinder : CsvContentFinder
{
    public string[] ColumnsExceptCoordinates;
    
    public string[] CoordinateColumns;

    public string XColumnName;
    public string YColumnName;

    public int XColumnIndex;
    public int YColumnIndex;

    public string LabelColumnName;
    public int LabelColumnIndex;

    public CsvGeoLocationFinder(string[] Columns, List<string[]> Rows)
    {
        this.Columns = Columns;
        this.Rows = Rows;

        GetCoordinateColumns();       
        ColumnsExceptCoordinates = Columns.Where(o => CoordinateColumns.Contains(o) == false).ToArray();

        if (Columns.Length == 1 )
        {
            Status = CsvContentFinderStatus.Failed;
            StatusMessageLines.Add("Geen kolommen gedetecteerd, controleer of de kolommen gescheiden zijn met het ; teken in het CSV bestand");
        }
        else if (CoordinateColumns.Length < 2)
        {
            Status = CsvContentFinderStatus.Failed;
            StatusMessageLines.Add("Geen coördinaten gedetecteerd, controleer het CSV bestand");
        }
        else{
            Status = CsvContentFinderStatus.Success;
        }
    }

    void GetCoordinateColumns()
    {
        //finding the coordinate columns
        var list = new List<string>();
        var row1 = Rows.First();

        for (int i = 0; i < Columns.Length; i++)
        {
            var col = row1[i];

            if (IsCoordinate(col))
            {
                var columnname = Columns[i];
                list.Add(columnname);

                double d = double.Parse(col);

                if(IsX(d))
                {
                    XColumnName = columnname;
                    XColumnIndex = i;
                }
                else if (IsY(d))
                {
                    YColumnName = columnname;
                    YColumnIndex = i;
                }

            }
        }
        CoordinateColumns = list.ToArray();
    }

    bool IsX(double d)
    {
        //range x = 7000 - 280000        
        //range lat 50.57222 - 53.62702
        return  d >= 7000 && d <= 280000 ||
                d >= 50.57222 && d <= 53.62702;       
    }

    bool IsY(double d)
    {
        //range y = 289000 - 629000
        //range lon 3.29804 - 7.57893
        return d >= 289000 && d <= 629000 ||
        d >= 3.29804 && d <= 7.57893;
    }

    public bool IsRd(double d)
    {
        return d >= 7000 && d <= 280000 ||
                d >= 289000 && d <= 629000;
    }

    public void SetlabelIndex(string labelcolumn)
    {
        LabelColumnIndex = Array.IndexOf( Columns, labelcolumn  );       
    }

    public static double GetCoordinateNumber(string numberString)
    {
        double num;
        bool parsed = ParseNumber (numberString, out num);
        if (parsed == false) throw new Exception("numberString must be a number");
        return num;
    }

    public static bool ParseNumber(string numberString, out double number)
    {
        return double.TryParse(numberString.Replace(",", "."), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number);
    }
   

    public static bool IsCoordinate(string stringdata)
    {
        double num;        
        if (ParseNumber(stringdata, out num) == false) return false;
        
        return IsCoordinate(num);                
    }

    public static bool IsCoordinate(double num)
    {
        //RD ranges
        //range x = 7000 - 280000
        //range y = 289000 - 629000

        //lat/lon ranges        
        //range lat 50.57222 - 53.62702
        //range lon 3.29804 - 7.57893
        return
                num >= 7000 && num <= 280000 ||
                num >= 289000 && num <= 629000 ||
                num >= 50.57222 && num <= 53.62702 ||
                num >= 3.29804 && num <= 7.57893;
    }

    internal string[] GetFiltersByColumn()
    {
        return Rows.Select(o => o[LabelColumnIndex]).Where( o=> string.IsNullOrEmpty(o) == false ).Distinct().Prepend("Toon alles").ToArray();
    }
}

