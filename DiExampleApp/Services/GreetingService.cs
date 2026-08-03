namespace DiExampleApp.Services;

public class GreetingService : IGreetingService
{
    int _messageCount =0;
    public string GreetingMessage(string name)
    {
        return $"Hello {name}, this message come from GreetingService.";
    }

    public int CountMessages()
    {
        return _messageCount++;
    }

}