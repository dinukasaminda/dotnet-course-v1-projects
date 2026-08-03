namespace DiExampleApp.Services;

public class SinhalaGreetingService : IGreetingService
{
    public string GreetingMessage(string name)
    {
        return $"Hello, {name} me message eka awe Sinhala Greeting Service eken";
    }

    public int CountMessages()
    {
        return 0;
    }
}