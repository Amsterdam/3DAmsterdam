namespace TileBakeLibrary
{
	public interface IConverter
	{
		public void SetSourcePath(string source);
		public void SetTargetPath(string target);

		public void Convert();
		public void Cancel();
	}
}