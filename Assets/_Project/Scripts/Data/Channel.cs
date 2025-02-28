using System.Collections.Generic;

// Interface to implement on Elements that add voltage to channel
public interface ISource
{ 
    public float GetOutput(int channelId);
}

// Interface to implement on Elements that listens to voltage (that needs information about all voltage in current channel without changing it, eg. Voltmeters and Cables)
public interface IListener
{
    public void SetInput(float voltage, int channelId);
}

// Interface to implement on Elements that takes voltage (eg. Receiving antennas) in equal proportions (if channel have 12 V total, and 2 Receivers, each of them get 6V)
public interface IReceiver : IListener
{
}

public class Channel
{
    private readonly int _id = 0;

    public Channel(int id)
    {
        _id = id;
    }
    
    private float _voltage = 0f;

    // List to keep track of voltage sources
    private readonly List<ISource> _sources = new List<ISource>();
    // List to keep track of voltage listeners
    private readonly List<IListener> _listeners = new List<IListener>();
    // List to keep track of voltage receivers
    private readonly List<IReceiver> _receivers = new List<IReceiver>();

    public void UpdateVoltage()
    {
        // Recalculate voltage
        _voltage = 0f;
        foreach (var source in _sources)
        {
            _voltage += source.GetOutput(_id);
        }

        // Update all listeners
        foreach (var listener in _listeners)
        {
            listener.SetInput(_voltage, _id);
        }

        // Update all Receivers
        foreach (var receiver in _receivers)
        {
            receiver.SetInput(_voltage / _receivers.Count, _id);
        }
    }

    public bool IsEmpty()
    {
        return _sources.Count == 0 && _listeners.Count == 0 && _receivers.Count == 0;
    }

    public void RemoveReferencesTo(Element element)
    {
        if (element is IListener listener)
        {
            while (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }

        if (element is IReceiver receiver)
        {
            while (_receivers.Contains(receiver))
            {
                _receivers.Remove(receiver);
            }
        }

        if (element is ISource source)
        {
            while (_sources.Contains(source))
            {
                _sources.Remove(source);
            }
        }
        UpdateVoltage();
    }

    public void AddVoltageSource(ISource source)
    {
        _sources.Add(source);
        UpdateVoltage();
    }

    public void RemoveVoltageSource(ISource source)
    {
        while (_sources.Contains(source))
        {
            _sources.Remove(source);
        }
        UpdateVoltage();
    }
        
    public void AddVoltageListener(IListener listener)
    {
        _listeners.Add(listener);
        UpdateVoltage();
    }

    public void RemoveVoltageListener(IListener listener)
    {
        while (_listeners.Contains(listener))
        {
            _listeners.Remove(listener);
        }
        UpdateVoltage();
    }
    
        
    public void AddVoltageReceiver(IReceiver receiver)
    {
        _receivers.Add(receiver);
        UpdateVoltage();
    }

    public void RemoveVoltageReceiver(IReceiver receiver)
    {
        while (_receivers.Contains(receiver))
        {
            _receivers.Remove(receiver);
        }
        UpdateVoltage();
    }
}