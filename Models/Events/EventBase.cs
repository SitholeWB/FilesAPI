namespace Models;

public class EventBase
{
    public EventBase()
    {
        OccuredOn = DateTime.Now;
    }

    protected DateTime OccuredOn
    {
        get;
        set;
    }
}