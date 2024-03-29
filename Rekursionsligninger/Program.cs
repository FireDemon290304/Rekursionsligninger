﻿using MathNet.Symbolics;
using System.Linq.Dynamic.Core;
using System.Globalization;
using Expr = MathNet.Symbolics.SymbolicExpression;
using System.Linq.Expressions;

namespace Rekursionsligninger;

class Program { static void Main() { Rekursion _ = new(); } }           // In

class Rekursion
{
    readonly string Ligning;
    readonly FloatingPoint y0;
    readonly int Iter;
    const float Tolerance = 1e-16f; // Set the tolerance for considering values equal

    readonly Expr eq;

    readonly Dictionary<string, FloatingPoint> Variables;

    public Rekursion()
    {
        (Ligning, y0, Iter) = GetInp(out var isCondition, out var condition);

        eq = Expr.Parse(Ligning);

        Variables = new() { { "y_n", y0 }, { "n", 0 } };

        Console.WriteLine($"\nEquation: {eq}");

        Console.WriteLine($"\t\ty_0:\t{y0.RealValue}");
        foreach ((int iteration, FloatingPoint item) in Test(isCondition, condition).Select((value, index) => (index, value)))
        { Console.WriteLine($"n = {iteration},\t\ty_{iteration + 1}:\t{item.RealValue}"); }

        Console.Write("..."); Console.ReadLine();                       // Out
    }

    IEnumerable<FloatingPoint> Test(bool isCondition, string? condition)
    {
        int n = 0;
        FloatingPoint currentY = y0;
        FloatingPoint nextY;

        while (isCondition ? EvalCondition(condition) : n < Iter)      // TODO: Add conditions for iteration like while y_n < 3
        {
            Variables["n"] = n;                                         // Assign current 'n'
            Variables["y_n"] = eq.Evaluate(Variables);                  // Assign new y0 based on res from current  
            try
            {
                nextY = Variables["y_n"].RealValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                break;
            }
            yield return nextY;

            // Check if the current and next values are approximately equal
            //if (Math.Abs(nextY - currentY) < Tolerance)
            //{
            //    // Terminate the loop if they are approximately equal
            //    Console.WriteLine("Terms y_(n+1) and y_n are approximately equal. Terminating loop");
            //    break;
            //}

            currentY = nextY;
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
            System.Linq.Expressions.Expression.Parameter(typeof(FloatingPoint), "y_n")
        };

        // This is a scope that provides variable names and their values to Dynamic LINQ
        Dictionary<string, FloatingPoint> values = new() { { "y_n", Variables["y_n"].RealValue } };

        // Parsing and executing the dynamic expression
        Delegate? dynamicCondition = DynamicExpressionParser.ParseLambda(
            parameters,
            typeof(bool),
            condition,
            values // Passing the scope here
        ).Compile();

        // Invoke the compiled lambda expression
#pragma warning disable CS8605
        return (bool)dynamicCondition.DynamicInvoke(Variables["y_n"].RealValue);
#pragma warning restore CS8605
    }

    static (string, FloatingPoint, int) GetInp(out bool isCondition, out string? condition)
    {
        // Default to null, indicating no condition
        isCondition = false;
        condition = null;

        Console.WriteLine("Accepted variables:\n\ty_n\n\tn\n");

        Console.Write("Recursive equation [Plaintext]: ");
        string inp = Console.ReadLine() ?? "";

        Console.Write("Beginning condition [y_0]: ");
        FloatingPoint initial = Expr.Parse(Console.ReadLine() ?? "-1").RealNumberValue;

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