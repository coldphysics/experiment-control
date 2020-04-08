namespace Controller.MainWindow
{
    public class ChildController:BaseController
    {
        protected BaseController parent;

        public ChildController(BaseController parent)
        {
            this.parent = parent;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }
    }
}
