using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConvertCoordinates;
using System.IO;

public class StreamreadOBJ : MonoBehaviour
{
	public int maxChractersPerFrame = 1000000;

    // OBJ File Tags
    const char O = 'o';
    const string V = "v";
    const string VT = "vt";
    const string VN = "vn";
    const string F = "f";
    const string MTLLIB = "mtllib";
    const string USEMTL = "usemtl";

    private const char faceSplitChar = '/';
    private const char lineSplitChar = '\n';
    private const char linePartSplitChar = ' ';

    [SerializeField]
    private GeometryBuffer buffer;

	private bool splitNestedObjects = false;
	private bool ignoreObjectsOutsideOfBounds = false;
	private Vector2RD bottomLeftBounds;
	private Vector2RD topRightBounds;
	private bool RDCoordinates = false;
	private bool flipFaceDirection = false;
	private bool flipYZ = false;
	private bool weldVertices = false;
	private int maxSubMeshes = 0;
	private bool tracing = false;
	private bool enableMeshRenderer = true;

	/// <summary>
	/// Disabled is the default. Otherwise SketchUp models would have a loooot of submodels (we cant use batching for rendering in WebGL, so this is bad for performance)
	/// </summary>
	public bool SplitNestedObjects { get => splitNestedObjects; set => splitNestedObjects = value; }
	public bool ObjectUsesRDCoordinates { get => RDCoordinates; set => RDCoordinates = value; }
	public bool IgnoreObjectsOutsideOfBounds { get => ignoreObjectsOutsideOfBounds; set => ignoreObjectsOutsideOfBounds = value; }
	public bool FlipYZ { get => flipYZ; set => flipYZ = value; }
	public int MaxSubMeshes { get => maxSubMeshes; set => maxSubMeshes = value; }
	public bool FlipFaceDirection { get => flipFaceDirection; set => flipFaceDirection = value; }
	public bool WeldVertices { get => weldVertices; set => weldVertices = value; }
	public bool EnableMeshRenderer { get => enableMeshRenderer; set => enableMeshRenderer = value; }
	public Vector2RD BottomLeftBounds { get => bottomLeftBounds; set => bottomLeftBounds = value; }
	public Vector2RD TopRightBounds { get => topRightBounds; set => topRightBounds = value; }
	public GeometryBuffer Buffer { get => buffer; }

	StreamReader streamReader;

	public void ReadOBJ(string filename)
    {
		StartCoroutine(StreamReadFile(filename));
    }

	IEnumerator StreamReadFile(string filename)
    {
		int objectCount = 0;
		char readChar;
		int characterCount = 0;
		streamReader = new StreamReader(filename, System.Text.Encoding.UTF8);
		while (true)
		{

			if (characterCount == maxChractersPerFrame)
			{
				characterCount = 0;
				yield return null;
			}


			if( !NextChar(out readChar));
            {

            }
			characterCount++;
            if (readChar == O)
            {
				objectCount++;
            }

		}
		Debug.Log(objectCount);
	}
	private void ReadLine()
    {

    }

	private bool NextChar(out char character)
    {
        if (streamReader.Peek() > -1)
        {
			character = (char)streamReader.Read();
			return true;
		}
		character = 'e';
		return false;
    }
}
