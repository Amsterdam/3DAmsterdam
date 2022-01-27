using System.Collections.Generic;

public class CsvContentFinder
{
    public enum CsvContentFinderStatus
    {
        Success,
        FailureWithSuggestion,
        Failed
    }

    public CsvContentFinderStatus Status = CsvContentFinderStatus.Success;

    public string[] Columns;
	public List<string[]> Rows;

    public List<string> StatusMessageLines = new List<string>();
}