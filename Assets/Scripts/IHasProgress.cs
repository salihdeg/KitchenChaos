public interface IHasProgress
{
    public event System.EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : System.EventArgs
    {
        public float progressNormalized;
    }
}
