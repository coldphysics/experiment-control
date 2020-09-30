namespace Errors.Error.ErrorItems
{
    /// <summary>
    /// An error item that does not get removed by the user interface. It has to be removed from the code manually
    /// (which should happen when the reason of the error goes away).
    /// </summary>
    public class StickyErrorItem : ConcreteErrorItem
    {
    }
}
