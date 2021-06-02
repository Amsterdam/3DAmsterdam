using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class CsvSettings
{
    public CsvSettingItem[] CsvItems;

    public void Load()
    {

    }

    public void Save()
    {

    }

}
public class CsvSettingItem
{
    public string Name;
    public string Url;
    public int XColumnIndex;
    public int YColumnIndex;
    public int LabelColumnIndex;
}


    public class CsvGeoLocationMapping
{
    public int? startAtRow;
    public int? title_index;
    public int? summary_index;
    public int? image_url_index;
    public int? yearstart_index;
    public int? yearend_index;
    public int? x_index;
    public int? y_index;    
}




public class CsvGeoLocation
{
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

    public CsvGeoLocationMapping Mapping;


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
            //store errors?
        }

        GetCoordinateColumns();

        ColumnsExceptCoordinates = Columns.Where(o => CoordinateColumns.Contains(o) == false).ToArray();
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

    //public static List<CsvGeoLocation> LoadCsv(string csv)
    //{

    //    var lines = CsvParser.ReadLines(csv, 0);

    //    var projects = new List<CsvGeoLocation>();

    //    foreach(var columns in lines)
    //    {
    //        if (columns.Length < 3)
    //        {
    //            Debug.Log($"A project needs at least 3 columns: {columns[0]}");
    //            continue;
    //            throw new Exception("A project needs at least 3 columns ");
    //        }

    //        var project = new CsvGeoLocation();

    //        project.title = mapping.title_index != null ? columns[mapping.title_index.Value] : "";
    //        project.summary = mapping.summary_index != null ? columns[mapping.summary_index.Value] : "";
    //        project.image_url = mapping.image_url_index != null ? columns[mapping.image_url_index.Value] : "";
    //        project.yearstart = mapping.yearstart_index != null ? int.Parse(columns[mapping.yearstart_index.Value]) : 0;
    //        project.yearend = mapping.yearend_index != null ? int.Parse(columns[mapping.yearend_index.Value]) : 0;

    //        if (mapping.longitude_index != null && mapping.latitude_index != null)
    //        {
    //            try
    //            {
    //                var lon_index = mapping.longitude_index.Value;
    //                var lat_index = mapping.latitude_index.Value;
    //                var lon = double.Parse(columns[lon_index]);
    //                var lat = double.Parse(columns[lat_index]);
    //                var rd = ConvertCoordinates.CoordConvert.WGS84toRD(lon, lat);
    //                project.x = rd.x;
    //                project.y = rd.y;
    //            }
    //            catch
    //            {
    //                Debug.Log("Error parsing the x/y coordinates");
    //            }
    //        }
    //        else
    //        {
    //            project.x = double.Parse(columns[mapping.x_index.Value]);
    //            project.y = double.Parse(columns[mapping.y_index.Value]);
    //        }

    //        projects.Add(project);

    //    }

    //    return projects;
    //}


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

