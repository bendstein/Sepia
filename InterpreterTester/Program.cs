using Interpreter.Lex;

foreach(var s in new string[]
{

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