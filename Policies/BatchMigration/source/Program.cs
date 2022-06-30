// See https://aka.ms/new-console-template for more information


Console.WriteLine("Hello, World!");

try
{
    var n = UserMigration.Uploader.RunAsync().GetAwaiter().GetResult();
    Console.WriteLine($"Loaded {n} records");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex.Message);
    Console.ResetColor();
}

Console.WriteLine("Press any key to exit");
Console.ReadKey();


