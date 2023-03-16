using Interpreter.Lex;

foreach(var s in new string[]
{
    //TODO, this should be a syntax error instead of the tokens Number:{123} and Id:{aa}
    @"123aa"
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