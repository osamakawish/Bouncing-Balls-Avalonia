using System;
using System.Windows;

namespace Bouncing_Balls_WPF.Models;

public delegate Vector ParametricFunction(double t);

public static class ParametricFunctionExt
{
    public static ParametricFunction Derivative(this ParametricFunction parametric, double halfDelta = 0.005)
        => t => (parametric(t + halfDelta) - parametric(t - halfDelta)) / (2 * halfDelta);

    public static ParametricFunction NthDerivative(this ParametricFunction parametric, byte n = 1, double halfDelta = 0.005) 
        => n switch { 
            0 => parametric,
            1 => parametric.Derivative(halfDelta),
            _ => parametric.Derivative(halfDelta).NthDerivative((byte)(n - 1), halfDelta)
        };

    /// <summary>
    /// A parametric function which stores the normal/perpendicular vector (in the counter-clockwise direction) at every point on the curve.
    /// </summary>
    /// <param name="parametric"></param>
    /// <param name="halfDelta"></param>
    public static ParametricFunction Normal(this ParametricFunction parametric, double halfDelta = 0.05)
    {
        var derivative = parametric.Derivative(halfDelta);
        return t => new(-derivative(t).Y, derivative(t).X);
    }

    public static double FindCriticalPoint(this ParametricFunction parametric, byte steps = 8)
    {
        throw new NotImplementedException();
    }

    public static double FindZero(this ParametricFunction parametric, byte steps = 8)
    {
        throw new NotImplementedException();
    }
}