using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Csv parser using regular expression
/// Seperator being used is a semicolon
/// </summary>
public class CsvParser
{    
    private static string splitPattern = @";(?=(?:[^""]*""[^""]*"")*[^""]*$)";


    /// <summary>
    /// Reads a csv string and returns a list of rows and columns
    /// </summary>
    /// <param name="csv">the csv string</param>
    /// <param name="startfromrow">Start reading from given row index</param>
    /// <returns></returns>
    public static List<string[]> ReadLines(string csv, int startfromrow)
    {

        var lines = csv.Split('\n');

        var rows = new List<string[]>();

        for (int i = startfromrow; i < lines.Length; i++)
        {            
            var line = lines[i].Trim();

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var cells = Regex.Split(line, splitPattern);

            for(int c=0; c<cells.Length;c++)
            {                
                cells[c] = cells[c].Trim().Trim('"');
            }
            rows.Add(cells);
        }

        return rows;
    }


}
