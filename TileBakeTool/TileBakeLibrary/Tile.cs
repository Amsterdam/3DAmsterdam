/*
*  Copyright (C) X Gemeente
*              	 X Amsterdam
*				 X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://joinup.ec.europa.eu/software/page/eupl
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TileBakeLibrary.Coordinates;

namespace TileBakeLibrary
{/// <summary>
/// contains all cityobjects and settings for an exportTile
/// </summary>
	class Tile
	{
		public Vector2Double position = new Vector2Double(); //Bottom left (RD coordinates)
		public Vector2 size = new Vector2(); //Width and height (RD coordinates)
		public string filePath = "";

		private List<SubObject> subObjects = new List<SubObject>();
		public List<SubObject> SubObjects { get => subObjects; }

		private bool SwapSubObjectWithSameID(SubObject subObject)
		{
			for (int i = 0; i < SubObjects.Count; i++)
			{
				if (SubObjects[i].id == subObject.id)
				{
					Console.WriteLine($"Replaced {subObject.id}");
					SubObjects[i] = subObject;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Add a subobject to this tile without appending the geometry to the tile geometry
		/// </summary>
		/// <param name="replaceSameID">Replace existing subobjects that share the same ID</param>
		public void AddSubObject(SubObject subObject, bool replaceSameID)
		{
			if (replaceSameID)
			{
                if (SwapSubObjectWithSameID(subObject))
                {
					return;
				}
				subObjects.Add(subObject);
				return; 
			}
			bool alreadyExists = false;
			string id = subObject.id;
            for (int i = 0; i < subObjects.Count; i++)
            {
                if (id == subObjects[i].id)
                {
					alreadyExists = true;
					break;
                }
            }
            if (!alreadyExists)
            {
				subObjects.Add(subObject);
			}
		}		
	}
}
