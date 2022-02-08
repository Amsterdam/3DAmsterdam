using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    /// <param name="startFromRow">Start reading from given row index</param>
    /// <returns></returns>
    public static List<string[]> ReadLines(string csv, int startFromRow)
    {
        var lines = csv.Split('\n');
        var rows = new List<string[]>();

        for (int i = startFromRow; i < lines.Length; i++)
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

    public static IEnumerator StreamReadLines(string csvFilePath, int startFromRow, int maxLinesPerFrame, Action<int> totalLinesFound, Action<int> currentlyAtLine, Action<List<string[]>> readAllRows)
    {     
        int lineNr = 0;
        List<string[]> rows = new List<string[]>();
        int totalLines = 0;

        using (StreamReader streamReader = File.OpenText(csvFilePath))
        {
            //Quick pass to count the newlines:
            while (streamReader.ReadLine() != null)
            {
                totalLines++;
            }
            totalLinesFound.Invoke(totalLines);

            //Reset streamreader to read the actual lines
            streamReader.DiscardBufferedData();
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (streamReader.Peek() >= 0)
            {
                var readLine = streamReader.ReadLine();
                var line = readLine.Trim();
                lineNr++;

                if (lineNr % maxLinesPerFrame == 0)
                {
                    currentlyAtLine.Invoke(lineNr);
                    yield return new WaitForEndOfFrame();
                }
                if (lineNr < startFromRow) continue;

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var cells = Regex.Split(line, splitPattern);
                for (int c = 0; c < cells.Length; c++)
                {
                    cells[c] = cells[c].Trim().Trim('"');
                }

			    rows.Add(cells);
            }
        }

        readAllRows.Invoke(rows);
    }
}
