namespace Communication.Interfaces.Buffer
{
    public interface IOutputHandler
    {
        void Stop();
        void IterateAndSave(bool once);
        void Start();
        //void StartRun();
        int GlobalCounter();
    }
}
