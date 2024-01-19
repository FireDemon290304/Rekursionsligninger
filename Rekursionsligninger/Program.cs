using MathNet.Symbolics;
using System.Linq.Dynamic.Core;
using System.Globalization;
using Expr = MathNet.Symbolics.SymbolicExpression;
using System.Linq.Expressions;

namespace Rekursionsligninger;

class Program { static void Main() { Rekursion _ = new(); } }           // In

class Rekursion
{
    readonly string Ligning;
    readonly float y0;
    readonly int Iter;

    readonly Expr eq;

    readonly Dictionary<string, FloatingPoint> Variables;

    public Rekursion()
    {
        (Ligning, y0, Iter) = GetInp(out var isCondition, out var condition);

        eq = Expr.Parse(Ligning);

        Variables = new() { { "y_n", y0 }, { "n", 0 } };

        Console.WriteLine($"\nEquation: {Ligning}");

        Console.WriteLine($"\t\ty_0:\t{y0}");
        foreach ((int iteration, Expr item) in Test(isCondition, condition).Select((value, index) => (index, value)))
        { Console.WriteLine($"n = {iteration},\t\ty_{iteration + 1}:\t{item.RealNumberValue}"); }

        Console.Write("..."); Console.ReadLine();                       // Out
    }

    IEnumerable<Expr> Test(bool isCondition, string? condition)
    {
        int n = 0;
        while (isCondition ? EvalCondition(condition) : n < Iter)      // TODO: Add conditions for iteration like while y_n < 3
        {
            // y_n=-1+(1+h)^n does not work for some reason
            Variables["n"] = n;                                         // Assign current 'n'
            Variables["y_n"] = eq.Evaluate(Variables);                  // Assign new y0 based on res from current
            yield return Variables["y_n"].RealValue;                    // Return new value of y0
            n++;
        }
    }

    bool EvalCondition(string? condition)
    {
        // If condition is null, stop
        if (condition == null) return false;

        // This sets up the parameter 'y_n' for the Dynamic LINQ library
        var parameters = new ParameterExpression[]
        {
            System.Linq.Expressions.Expression.Parameter(typeof(float), "y_n")
        };

        // This is a scope that provides variable names and their values to Dynamic LINQ
        var values = new Dictionary<string, object> { { "y_n", Variables["y_n"].RealValue } };

        // Parsing and executing the dynamic expression
        var dynamicCondition = DynamicExpressionParser.ParseLambda(
            parameters,
            typeof(bool),
            condition,
            values // Passing the scope here
        ).Compile();

        // Invoke the compiled lambda expression
        float? realValue = (float)Variables["y_n"].RealValue;
        return (bool)dynamicCondition.DynamicInvoke(realValue);
    }

    static (string, float, int) GetInp(out bool isCondition, out string? condition)
    {
        isCondition = false;
        condition = null; // Default to null, indicating no condition

        Console.WriteLine("Accepted variables:\n\ty_n\n\tn\n");

        Console.Write("Recursive equation [Plaintext]: ");
        string inp = Console.ReadLine() ?? "";

        Console.Write("Beginning condition [y_0]: ");
        float initial = float.Parse(Console.ReadLine() ?? "-1", CultureInfo.InvariantCulture);

        Console.Write("Iterations or condition [int or bool expression]: ");
        var iterOrCondition = Console.ReadLine() ?? "";

        // Check if the input is an integer, if not, treat it as a condition
        if (int.TryParse(iterOrCondition, out int iterations))
        {
            return (inp, initial, iterations);
        }
        else
        {
            isCondition = true;
            condition = iterOrCondition;
            return (inp, initial, -1); // Iter will be unused
        }
    }
}