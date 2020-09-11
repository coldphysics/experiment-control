
namespace Controller.OutputVisualizer.Export.Abstract
{
    /// <summary>
    /// Represents exporters that have some sort of a file as output (in contrast to a stream, a database table, ...)
    /// </summary>
    public interface IFileBasedExporter
    {
        /// <summary>
        /// The path of the file to be stored as the result of the export
        /// </summary>
        string OutputPath
        {
            set;
            get;
        }
    }
}
