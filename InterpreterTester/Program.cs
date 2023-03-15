using Interpreter.Lex;

foreach(var s in new string[]
{
    "",
    "+",
    "5",
    "5.11",
    "0b001",
    "0x51F",
    "5+3+2.1**4.1"
})
{
    try
    {
        Scanner scanner = new Scanner(s);

        foreach (var token in scanner)
            Console.WriteLine(token);

        Console.WriteLine("---------");
    }
    catch (Exception e) 
    {
        Console.WriteLine(e.ToString());
    }
}