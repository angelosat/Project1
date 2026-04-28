namespace Project1.Framework;

public interface IStructIdInt<T>
{
    int Value { get; }
    static abstract T Create(int value);
}
