using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Utilities
{
    public static class StringManipulation
    {
        public static double ParseNextDouble(string line, char seperator, int startpostition, out int endposition)
        {
            double currentvalue = 0;
            double currentSignValue = 1;
            int charactercounter = 0;
            int charNumber;

            double mantissaValue = 1;
            bool isinDecimal = false;

            char character;

            for (charNumber = startpostition; charNumber < line.Length; charNumber++)
            {
                character = line[charNumber];
                if (char.IsDigit(character))
                {
                    charactercounter++;
                    currentvalue = (currentvalue * 10) + (int)char.GetNumericValue(character);
                    if (isinDecimal)
                    {
                        mantissaValue *= 10;
                    }
                    continue;
                }
                else if (character == 'n')
                {
                    //if we find null return 0
                    endposition = startpostition;
                    return 0;
                }
                else if (character == seperator && charactercounter > 0)
                {
                    endposition = charNumber + 1;
                    if (isinDecimal)
                    {
                        return (currentvalue * currentSignValue) / mantissaValue;
                    }
                    else
                    {
                        return currentvalue * currentSignValue;
                    }
                }
                else if (character == '.')
                {
                    isinDecimal = true;
                }
                else if (character == '-')
                {
                    currentSignValue = -1;
                    continue;
                }
                else if (character == '"')
                {
                    if (charactercounter > 0)
                    {
                        break;
                    }
                }
            }
            endposition = charNumber + 1;
            if (isinDecimal)
            {
                return (currentvalue * currentSignValue) / mantissaValue;
            }
            return currentvalue * currentSignValue;

        }

        public static int ParseNextInt(string line, char seperator, int startpostition, out int endposition)
        {
            int currentvalue = 0;
            int currentSignValue = 1;
            int charactercounter = 0;
            int charNumber;
            char character;
            for (charNumber = startpostition; charNumber < line.Length; charNumber++)
            {
                character = line[charNumber];
                if (char.IsDigit(character))
                {
                    charactercounter++;
                    currentvalue = (currentvalue * 10) + (int)char.GetNumericValue(character);
                    continue;
                }
                if (character == seperator && charactercounter > 0)
                {
                    endposition = charNumber + 1;
                    return currentvalue * currentSignValue;
                }
                if (character == '-')
                {
                    currentSignValue = -1;
                    continue;
                }
            }

            endposition = charNumber + 1;
            return currentvalue * currentSignValue;
        }
    }
}