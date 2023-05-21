namespace Amplitude.Net;

public struct Optional<T>
{
    public bool HasValue { get; }
    private T value;
    public T Value
    {
        get
        {
            if (HasValue)
                return value;
            
            throw new InvalidOperationException();
        }
    }

    public T? ValueOrDefault
    {
        get
        {
            if (HasValue)
                return value;

            return default;
        }
    }

    public Optional(T value)
    {
        this.value = value;
        HasValue = true;
    }

    public static explicit operator T(Optional<T> optional)
    {
        return optional.Value;
    }
    public static implicit operator Optional<T>(T value)
    {
        return new Optional<T>(value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Optional<T>)
            return Equals((Optional<T>)obj);
        
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(value, HasValue);
    }

    public bool Equals(Optional<T> other)
    {
        if (HasValue && other.HasValue)
            return Equals(value, other.value);
        
        return HasValue == other.HasValue;
    }
}