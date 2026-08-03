namespace DiExampleApp.Services;

public interface IGreetingService
{
    string GreetingMessage(string name);

    int CountMessages();
    
}