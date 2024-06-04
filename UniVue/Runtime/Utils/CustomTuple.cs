
namespace UniVue.Utils
{
    public sealed class CustomTuple<T1, T2>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public CustomTuple() { }

        public CustomTuple(T1 t1, T2 t2)
        {
            Item1 = t1; Item2 = t2;
        }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
        }
    }

    public sealed class CustomTuple<T1, T2, T3>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
        }
    }

    public sealed class CustomTuple<T1, T2, T3, T4>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
            Item4 = default;
        }
    }

    public sealed class CustomTuple<T1, T2, T3, T4, T5, T6>
    {
        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public T3 Item3 { get; set; }

        public T4 Item4 { get; set; }

        public T5 Item5 { get; set; }

        public T6 Item6 { get; set; }

        public void Dispose()
        {
            Item1 = default;
            Item2 = default;
            Item3 = default;
            Item4 = default;
            Item5 = default;
            Item6 = default;
        }
    }
}
