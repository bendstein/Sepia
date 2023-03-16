using Interpreter.Lex;
using System.Diagnostics;

Stopwatch stopwatch = new Stopwatch();

foreach(var s in new string[]
{
    @"
function x(int y) {
    function /* x(int y) {
        var z = y;
        return z;
    }/**/

    var z = y;
    return z;
}

function x(int y) {
    function x(int y) {
        var z = y;
        return z;
    }

    var z = y;
    return z;
}

"
})
{
    try
    {
        Scanner scanner = new Scanner(s);

        stopwatch.Restart();

        List<Token> tokens = scanner.Scan().ToList();

        stopwatch.Stop();

        Console.WriteLine($"Finished tokenizing the following input:\r\n{s}");
        Console.WriteLine();
        Console.WriteLine("Tokens:");

        foreach(var token in tokens)
        {
            Console.WriteLine(token);
            Console.WriteLine("---------");
        }
        //foreach (var token in scanner)
        //{
        //    Console.WriteLine(token);
        //    Console.WriteLine("---------");
        //}

        Console.WriteLine();
        Console.WriteLine($"Elapsed Time: {stopwatch.Elapsed.TotalMilliseconds}ms.");
    }
    catch (Exception e) 
    {
        Console.WriteLine(e.Message);
    }

    Console.WriteLine("[oooooooooooooooooooo]");
    Console.WriteLine("[oooooooooooooooooooo]");

}