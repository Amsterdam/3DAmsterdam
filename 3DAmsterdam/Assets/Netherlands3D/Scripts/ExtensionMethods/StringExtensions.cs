using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using System.IO;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

public static class StringExtensions
{
       
    
    /// <summary>
    /// Replace the template string and fill in the x and y values
    /// </summary>
    /// <param name="template">The template string</param>
    /// <param name="x">double value x</param>
    /// <param name="y">double value y</param>
    /// <returns>The replaced template string</returns>
    public static string ReplaceXY(this string template, double x, double y)
    {
        StringBuilder sb = new StringBuilder(template);
        sb.Replace("{x}", $"{x}");
        sb.Replace("{y}", $"{y}");
        return sb.ToString();        
    }

    /// <summary>
    /// Get the RD coordinate of a files string
    /// </summary>
    /// <param name="filepath">string filepath</param>
    /// <returns>The Vector3RD coordinate</returns>
	public static Vector3RD GetRDCoordinate(this string filepath)
	{
		var numbers = new Regex(@"(\d{4,6})");

		var matches = numbers.Matches(filepath);
		if(matches.Count == 2)
        {
			return new Vector3RD()
			{
				x = double.Parse(matches[0].Value),
				y = double.Parse(matches[1].Value)
			};
		}
        else
        {
			throw new Exception($"Could not get RD coordinate of string: {filepath}");
		}
	}


    public static string ToInvariant(this double d)
    {
        return d.ToString(CultureInfo.InvariantCulture);
    }

}
