namespace Errors.Error
{
    /// <summary>
    /// An error item that does not get removed by the user interface. It has to be removed from the code manually
    /// (which should happen when the reason of the error goes away).
    /// </summary>
    /// <seealso cref="Errors.Error.ErrorItem" />
    public class StickyErrorItem:ErrorItem
    {
    }
}
