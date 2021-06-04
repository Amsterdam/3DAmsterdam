﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;



public class CsvGeoLocation
{
    public enum CsvGeoLocationStatus
    {
        Success, FailedNoColumns, FailedNoCoordinates, SuccessWithErrors
    }

    public string[] Columns;
    public string[] ColumnsExceptCoordinates;

    public List<string[]> Rows;
    
    public string[] CoordinateColumns;

    public string XColumnName;
    public string YColumnName;

    public int XColumnIndex;
    public int YColumnIndex;

    public string LabelColumnName;
    public int LabelColumnIndex;

    public CsvGeoLocationStatus Status = CsvGeoLocationStatus.Success;
    public List<string> StatusMessageLines = new List<string>();

    public CsvGeoLocation(string csv)
    {
        var lines = CsvParser.ReadLines(csv, 0);
        Columns = lines.First();
        Rows = new List<string[]>();

        int columnscount = Columns.Length;

        for (int i=1; i< lines.Count; i++)
        {
            if(lines[i].Length == columnscount)
            {
                Rows.Add(lines[i]);
            }
            //TODO store errors in StatusMessageLines
        }

        GetCoordinateColumns();       

        ColumnsExceptCoordinates = Columns.Where(o => CoordinateColumns.Contains(o) == false).ToArray();

        if (Columns.Length == 1 )
        {
            Status = CsvGeoLocationStatus.FailedNoColumns;
            StatusMessageLines.Add("Geen kolommen gedetecteerd, controleer of de kolommen gescheiden zijn met het ; teken in het CSV bestand");
            return;
        }

        if (CoordinateColumns.Length < 2)
        {
            Status = CsvGeoLocationStatus.FailedNoCoordinates;
            StatusMessageLines.Add("Geen coördinaten gedetecteerd, controleer het CSV bestand");
            return;
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


    public static bool IsCoordinate(string stringdata)
    {
        double num;
        bool canConvert = double.TryParse(stringdata, out num);

        if (canConvert == false) return false;

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

