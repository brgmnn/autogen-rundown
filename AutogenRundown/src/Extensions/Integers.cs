namespace AutogenRundown.Extensions;

public static class Integers
{
    /// <summary>
    /// Cardinal numbers (one, two, three, four...) tell you how many of something there are.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string ToCardinal(int number)
        => number switch
        {
            1 => "one",
            2 => "two",
            3 => "three",
            4 => "four",
            5 => "five",
            6 => "six",
            7 => "seven",
            8 => "eight",
            9 => "nine",
            10 => "ten",

            _ => "many"
        };

    /// <summary>
    /// Ordinal numbers (first, second, third, fourth...) tell you the position or order of something in a sequence.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static string ToOrdinal(this int number) =>
        number switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            4 => "fourth",
            5 => "fifth",
            6 => "sixth",
            7 => "seventh",
            8 => "eighth",
            9 => "ninth",
            10 => "tenth",

            _ => "many"
        };
}
