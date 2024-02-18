using System;

namespace Quarter.Core.Models;

/// <summary>
/// Unique ID for aggregates
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate</typeparam>
public class IdOf<TAggregate> where TAggregate : IAggregate<TAggregate>
{
    public Guid Id { get; }

    private IdOf(Guid id)
    {
        Id = id;
    }

    public static IdOf<TAggregate> Of(Guid id)
        => new(id);

    public static IdOf<TAggregate> Of(string value)
    {
        if (!Guid.TryParse(value, out var id))
            throw new ArgumentException($"{value} is not a valid UUID");
        return new IdOf<TAggregate>(id);
    }

    public static IdOf<TAggregate> Random()
        => new(Guid.NewGuid());

    public static IdOf<TAggregate> None = new(Guid.Empty);

    public string AsString()
        => Id.ToString();

    public override string ToString()
        => Id.ToString();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        var other = (IdOf<TAggregate>)obj;
        return Id == other.Id;
    }

    public override int GetHashCode()
        => Id.GetHashCode();

    public static bool operator ==(IdOf<TAggregate>? left, IdOf<TAggregate>? right)
        => Equals(left, right);

    public static bool operator !=(IdOf<TAggregate>? left, IdOf<TAggregate>? right)
        => !Equals(left, right);
}
