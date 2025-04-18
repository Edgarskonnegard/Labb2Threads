namespace Labb2Threads;

public class Car
{
    public int Speed { get; set; } = 120;
    public int SpeedLimit = 120;
    
    public string Name { get; set; }
    public int Distance { get; set; } = 50000;

    public bool OverFinishLine { get; set; } = false;

    public void Drive()
    {
        //System.Console.WriteLine(Name);
        Task.Delay(1000);
        Distance-=Speed;
    }
    
}
