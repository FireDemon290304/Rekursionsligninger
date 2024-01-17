using System.Linq;
using System.Runtime.Versioning;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;

namespace Rekursionsligninger;

class Program { static void Main() { Rekursion _ = new(); } }           // In

class Rekursion
{
    string Ligning;
    float y0;
    int Iter;

    Expr eq;
    Dictionary<string, FloatingPoint> Variables;

    bool Cont;

    public Rekursion()
    {
        (Ligning, y0, Iter) = GetInp();
        
        eq = Expr.Parse(Ligning);
        Variables = new() { { "y_n", y0}, { "n", -1} };
        
        Console.WriteLine($"\ny0 = {y0},\t\t{Ligning}\n");

        foreach ((int iteration, Expr item) in Test().Select((value, index) => (index, value)))
        { Console.WriteLine($"n = {iteration},\t\ty_{iteration}: {item.RealNumberValue}"); }

        Console.Write("..."); Console.ReadLine();                       // Out
    }

    IEnumerable<Expr> Test()
    {
        for (int n = 0; n < Iter; n++)
        {
            Variables["n"] = n;                                         // Assign current 'n'
            Variables["y_n"] = eq.Evaluate(Variables);                  // Assign new y0 based on res from current
            yield return Variables["y_n"].RealValue;                    // Return new value of y0
        }
    }

    static (string, float, int) GetInp()
    {
        Console.WriteLine("Accepted variables:\n\ty_n\n\tn\n");

        Console.Write("Recursive equation [Plaintext]: ");
        string inp = Console.ReadLine() ?? "";

        Console.Write("Beginning condition [y_0]: ");
        float initial = float.Parse(Console.ReadLine() ?? "-1");

        Console.Write("Iterations [int > 0]: ");
        int iterations = int.Parse(Console.ReadLine() ?? "-1");

        return (inp, initial, iterations);
    }
}