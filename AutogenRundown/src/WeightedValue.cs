namespace AutogenRundown
{
    internal class WeightedValue<T> : Generator.ISelectable
    {
        public double Weight { get; set; } = 1.0;

        public T? Value { get; set; }
    }
}
