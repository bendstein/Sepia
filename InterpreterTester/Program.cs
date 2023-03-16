using Interpreter.Lex;

foreach(var s in new string[]
{
    @"
function x(int y) {
    var z = y;
    return z;
}

"
})
{
    try
    {
        Scanner scanner = new Scanner(s);
        
        foreach (var token in scanner)
        {
            Console.WriteLine(token);
            Console.WriteLine("---------");
        }

    }
    catch (Exception e) 
    {
        Console.WriteLine(e.Message);
    }

    Console.WriteLine("[oooooooooooooooooooo]");

}