namespace Utility.EventSystem
{
    public interface IEventRegister
    {
        public void Register(object obj);
        public void Unregister(object obj);
    }
}