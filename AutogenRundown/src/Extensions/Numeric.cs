namespace AutogenRundown.Extensions;

public static class Numeric
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public static bool ApproxEqual(this double a, double b, double tolerance = 1e-10)
        => Math.Abs(a - b) < tolerance;
}
