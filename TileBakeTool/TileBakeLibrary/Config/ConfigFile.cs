
public class ConfigFile
{
	public string sourceFolder { get; set; }
	public string outputFolder { get; set; }
	public bool replaceExistingObjects { get; set; }
	public string identifier { get; set; }
	public string removePartOfIdentifier { get; set; }
	public float lod { get; set; }
	public string tilingMethod { get; set; }
	public bool brotliCompression { get; set; }
	public CityObjectFilter[] cityObjectFilters { get; set; }
}

public class CityObjectFilter
{
	public string objectType { get; set; }
	public int defaultSubmeshIndex { get; set; }
	public AttributeFilter[] attributeFilters { get; set; }
	public float maxVerticesPerSquareMeter { get; set; }
}

public class AttributeFilter
{
	public string attributeName { get; set; }
	public ValueToSubmesh[] valueToSubMesh { get; set; }
}

public class ValueToSubmesh
{
	public string value { get; set; }
	public int submeshIndex { get; set; }
}
