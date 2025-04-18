using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Labb2Threads;

class Program
{
    static Mutex mutex = new Mutex();
    static bool isWinner = false;
    static Random random = new Random();
    static string input = "";

    static void Main(string[] args)
    {
        StartAsyncRace().Wait();
    }

    static async Task StartAsyncRace()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Car car1 = new Car { Name = "röd bil" };
        Car car2 = new Car { Name = "gul bil" };
        Car car3 = new Car { Name = "blå bil" };

        var car1RaceTask = Race(car1, stopwatch);
        var car2RaceTask = Race(car2, stopwatch);
        var car3RaceTask = Race(car3, stopwatch);

        var input = "";
        var cts = new CancellationTokenSource();
        var inputTask = Task.Run(() => HandleInputAsync(new List<Car> { car1, car2, car3 }, cts.Token));

        ICollection<Task> raceTasks = new List<Task> { car1RaceTask, car2RaceTask, car3RaceTask };
        while (raceTasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(raceTasks);
            if (finishedTask == car1RaceTask)
            {
            }
            else if (finishedTask == car2RaceTask)
            {
            }
            else if (finishedTask == car3RaceTask)
            {
            }
            await finishedTask;
            raceTasks.Remove(finishedTask);
        }

        cts.Cancel(); // Stoppa input-tasken när racet är klart
        await inputTask; // Vänta på att input-tasken avslutas
        stopwatch.Stop();
    }

    static async Task Race(Car car, Stopwatch stopwatch)
    {
        System.Console.WriteLine($"{car.Name} Started");
        long lastEventTime = 0;
        while (!car.OverFinishLine)
        {
            if (stopwatch.ElapsedMilliseconds - lastEventTime >= 10000) // Händelser var 10:e sekund
            {
                //Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine($"EVENT: {car.Name} at {stopwatch.Elapsed}");
                //Console.ResetColor();
                await Events(car, stopwatch);
                lastEventTime = stopwatch.ElapsedMilliseconds;
            }
            car.Drive();
            if (car.Distance <= 0)
            {
                mutex.WaitOne();
                car.OverFinishLine = true;
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine(isWinner ? $"Car crossed finnish line: {car.Name} {stopwatch.Elapsed}" : $"AND THE WINNER IS: {car.Name} {stopwatch.Elapsed}");
                isWinner = true;
                mutex.ReleaseMutex();
            }
            await Task.Delay(100); 
        }
    }

    static async Task HandleInputAsync( List<Car> cars, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            input = await StatusReport(input, cars);
        }
    }

    static async Task<string> StatusReport(string input, ICollection<Car> cars)
    {
        var newKey = await ReadKeyAsync();
        if (newKey != null)
        {
            if (newKey.Value.Key == ConsoleKey.Enter)
            {
                await Status(cars);
                input = ""; 
            }
            else
            {
                input += newKey.Value.KeyChar;
                if (input.Length > 6)
                {
                    input = input.Substring(input.Length - 6);
                }
                if (input.ToLower() == "status")
                {
                    await Status(cars);
                    input = ""; 
                }
            }
        }
        return input;
    }

    static async Task<ConsoleKeyInfo?> ReadKeyAsync()
    {
        //await Task.Delay(10);
        if (Console.KeyAvailable)
        {
            return Console.ReadKey(); 
        }
        return null;
    }

    static async Task Status(ICollection<Car> cars)
    {
        System.Console.WriteLine("STATUS");
        foreach (Car car in cars)
        {
            System.Console.WriteLine($"{car.Name}: Distance = {car.Distance}m, Speed = {car.Speed}m/s");
        }
        System.Console.WriteLine();
    }

    static async Task Events(Car car, Stopwatch stopwatch)
    {
        //Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.Yellow;
        var randomEvent = random.Next(1, 51);
        System.Console.WriteLine($"chance {car.Name} event {randomEvent}");
        if (randomEvent <= 18)
        {
            mutex.WaitOne();
            if (randomEvent == 1)
            {
                OutOfGas(car, stopwatch);
            }
            else if (randomEvent <= 3)
            {
                FlatTire(car, stopwatch);
            }
            else if (randomEvent <= 8)
            {
                Birdie(car, stopwatch);
            }
            else if (randomEvent <= 18)
            {
                EngineFailure(car, stopwatch);
            }
            mutex.ReleaseMutex();
        }
        Console.ResetColor();
    }

    static void OutOfGas(Car car, Stopwatch stopwatch)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"OutOfGas {car.Name} {stopwatch.Elapsed}");
        Thread.Sleep(15000); // Pausar Drive i 15 sekunder
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"{car.Name} is back on the road! {stopwatch.Elapsed}");
    }

    static void Birdie(Car car, Stopwatch stopwatch)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"Birdie {car.Name} {stopwatch.Elapsed}");
        Thread.Sleep(10000); // Pausar Drive i 10 sekunder
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"{car.Name} is back on the road! {stopwatch.Elapsed}");
    }

    static void FlatTire(Car car, Stopwatch stopwatch)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"FlatTire {car.Name} {stopwatch.Elapsed}");
        Thread.Sleep(5000); // Pausar Drive i 5 sekunder
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"{car.Name} is back on the road! {stopwatch.Elapsed}");
    }

    static void EngineFailure(Car car, Stopwatch stopwatch)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"EngineFailure {car.Name} {stopwatch.Elapsed}");
        car.Speed -= car.Speed > 0 ? 1 : 0;
    }
}