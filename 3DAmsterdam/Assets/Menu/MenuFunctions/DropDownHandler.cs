//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class DropDownHandler : MonoBehaviour
//{
//    List<string> days = new List<string>() {"01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
//                                            "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
//                                            "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
//                                            "31"};

//    List<string> months = new List<string>() {"Januari", "Februari", "Maart", "April", "Mei", "Juni", "Juli",
//                                              "Augustus", "September", "Oktober", "November", "December"};

//    List<string> years = new List<string>() {"1990", "1991", "1992", "1993", "1994", "1995", "1996", "1997", "1998",
//                                             "1999", "2000", "2001", "2002", "2003", "2004", "2005", "2006", "2007",
//                                             "2008", "2009", "2010", "2011", "2012", "2013", "2014", "2015", "2016",
//                                             "2017", "2018", "2019", "2020" };

//    List<string> weatherTypes = new List<string>() { "Type1", "Type2", "Type3", "Type4", "Type5", "Type6", "Type7" };

//    public Dropdown[] dropDowns;


//    private void Start()
//    {
//        dropDowns[0].AddOptions(days);
//        dropDowns[1].AddOptions(months);
//        dropDowns[2].AddOptions(years);
//        dropDowns[3].AddOptions(weatherTypes);
//    }
//}
