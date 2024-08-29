using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Services
{
    public interface IListenerService
    {
        void AddListener(Listener listener);
        IEnumerable<Listener> GetListeners();

        Listener GetListener(string name);
        void RemoveListener(Listener listener);
    }
}

public class ListenerService : IListenerService
{
    private readonly List<Listener> _listeners = new();
    public void AddListener(Listener listener)
    {
        _listeners.Add(listener);
    }
    public IEnumerable<Listener> GetListeners()
    {
        return _listeners;
    }
    public Listener GetListener(string name)
    {
        return GetListeners().FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void RemoveListener(Listener listener)
    {
        _listeners.Remove(listener);
    }
}
