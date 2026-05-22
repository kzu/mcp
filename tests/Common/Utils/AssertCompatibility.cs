using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using NUnitAssert = NUnit.Framework.Assert;
using NUnitFramework = NUnit.Framework;

namespace ModelContextProtocol.Tests.Utils;

public static class AssertCompatibility
{
    public static void True(bool condition) => NUnitAssert.That(condition, NUnitFramework.Is.True);

    public static void True(bool condition, string message) => NUnitAssert.That(condition, NUnitFramework.Is.True, message);

    public static void True(bool? condition) => NUnitAssert.That(condition, NUnitFramework.Is.True);

    public static void True(bool? condition, string message) => NUnitAssert.That(condition, NUnitFramework.Is.True, message);

    public static void False(bool condition) => NUnitAssert.That(condition, NUnitFramework.Is.False);

    public static void False(bool condition, string message) => NUnitAssert.That(condition, NUnitFramework.Is.False, message);

    public static void False(bool? condition) => NUnitAssert.That(condition, NUnitFramework.Is.False);

    public static void False(bool? condition, string message) => NUnitAssert.That(condition, NUnitFramework.Is.False, message);

    public static void Equal<T>(T expected, T actual) => NUnitAssert.That(actual, NUnitFramework.Is.EqualTo(expected));

    public static void Equal<T>(T expected, T actual, string message) =>
        NUnitAssert.That(actual, NUnitFramework.Is.EqualTo(expected), message);

    public static void Equal<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
        NUnitAssert.That(actual, NUnitFramework.Is.EqualTo(expected));

    public static void Equal(byte[] expected, ReadOnlyMemory<byte> actual) =>
        NUnitAssert.That(actual.ToArray(), NUnitFramework.Is.EqualTo(expected));

    public static void Equal(ReadOnlyMemory<byte> expected, ReadOnlyMemory<byte> actual) =>
        NUnitAssert.That(actual.ToArray(), NUnitFramework.Is.EqualTo(expected.ToArray()));

    public static void NotEqual<T>(T expected, T actual) => NUnitAssert.That(actual, NUnitFramework.Is.Not.EqualTo(expected));

    public static void Same(object? expected, object? actual) => NUnitAssert.That(actual, NUnitFramework.Is.SameAs(expected));

    public static void NotSame(object? expected, object? actual) => NUnitAssert.That(actual, NUnitFramework.Is.Not.SameAs(expected));

    public static void Null([MaybeNull] object? value) => NUnitAssert.That(value, NUnitFramework.Is.Null);

    public static void NotNull([NotNull] object? value)
    {
        if (value is null)
        {
            NUnitAssert.Fail("Expected non-null value.");
            throw new InvalidOperationException("Unreachable");
        }
    }

    public static T NotNull<T>([NotNull] T? value) where T : struct
    {
        if (value is null)
        {
            NUnitAssert.Fail("Expected non-null value.");
            throw new InvalidOperationException("Unreachable");
        }

        return value.Value;
    }

    public static void Empty([MaybeNull] string? value) => NUnitAssert.That(value, NUnitFramework.Is.Null.Or.Empty);

    public static void Empty([MaybeNull] IEnumerable collection)
    {
        NUnitAssert.That(collection, NUnitFramework.Is.Not.Null);
        NUnitAssert.That(collection!.Cast<object?>().Any(), NUnitFramework.Is.False);
    }

    public static void Empty<T>([MaybeNull] IEnumerable<T>? collection) => NUnitAssert.That(collection, NUnitFramework.Is.Empty);

    public static void NotEmpty<T>([NotNull] IEnumerable<T>? collection)
    {
        if (collection is null)
        {
            NUnitAssert.Fail("Expected non-empty collection.");
            throw new InvalidOperationException("Unreachable");
        }

        NUnitAssert.That(collection, NUnitFramework.Is.Not.Empty);
    }

    public static void InRange<T>(T actual, T low, T high) where T : IComparable =>
        NUnitAssert.That(actual, NUnitFramework.Is.InRange(low, high));

    public static void Contains(string expectedSubstring, string? actualString) =>
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null.And.Contains(expectedSubstring));

    public static void Contains(string expectedSubstring, string? actualString, StringComparison comparisonType)
    {
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null);
        NUnitAssert.That(actualString!.Contains(expectedSubstring, comparisonType), NUnitFramework.Is.True);
    }

    public static void Contains(char expectedSubchar, string? actualString)
    {
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null);
        NUnitAssert.That(actualString!.Contains(expectedSubchar), NUnitFramework.Is.True);
    }

    public static void Contains<T>(T expected, IEnumerable<T> collection) =>
        NUnitAssert.That(collection, NUnitFramework.Does.Contain(expected));

    public static void Contains<T>(IEnumerable<T> collection, Predicate<T> filter) =>
        NUnitAssert.That(collection, NUnitFramework.Has.Some.Matches(filter));

    public static void DoesNotContain(string expectedSubstring, string? actualString)
    {
        if (actualString is null)
        {
            return;
        }

        NUnitAssert.That(actualString.Contains(expectedSubstring, StringComparison.Ordinal), NUnitFramework.Is.False);
    }

    public static void DoesNotContain(string expectedSubstring, string? actualString, StringComparison comparisonType)
    {
        if (actualString is null)
        {
            return;
        }

        NUnitAssert.That(actualString.Contains(expectedSubstring, comparisonType), NUnitFramework.Is.False);
    }

    public static void DoesNotContain(char expectedSubchar, string? actualString)
    {
        if (actualString is null)
        {
            return;
        }

        NUnitAssert.That(actualString.Contains(expectedSubchar), NUnitFramework.Is.False);
    }

    public static void DoesNotContain<T>(T expected, IEnumerable<T> collection) =>
        NUnitAssert.That(collection, NUnitFramework.Does.Not.Contain(expected));

    public static void DoesNotContain<T>(IEnumerable<T> collection, Predicate<T> filter) =>
        NUnitAssert.That(collection, NUnitFramework.Has.None.Matches(filter));

    public static void StartsWith(string expectedStart, string? actualString) =>
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null.And.StartsWith(expectedStart));

    public static void EndsWith(string expectedEnd, string? actualString) =>
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null.And.EndsWith(expectedEnd));

    public static void Matches(string pattern, string? actualString) =>
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null.And.Match(pattern));

    public static void Matches(Regex regex, string? actualString) =>
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null.And.Match(regex));

    public static T IsType<T>([NotNull] object? obj)
    {
        if (obj is null)
        {
            NUnitAssert.Fail($"Expected object of type {typeof(T).Name}.");
            throw new InvalidOperationException("Unreachable");
        }

        NUnitAssert.That(obj, NUnitFramework.Is.TypeOf<T>());
        return (T)obj;
    }

    public static T IsAssignableFrom<T>([NotNull] object? obj)
    {
        if (obj is null)
        {
            NUnitAssert.Fail($"Expected object assignable to type {typeof(T).Name}.");
            throw new InvalidOperationException("Unreachable");
        }

        NUnitAssert.That(obj, NUnitFramework.Is.InstanceOf<T>());
        return (T)obj;
    }

    public static T Single<T>([NotNull] IEnumerable<T>? collection) where T : notnull
    {
        if (collection is null)
        {
            NUnitAssert.Fail("Expected collection with exactly one item.");
            throw new InvalidOperationException("Unreachable");
        }

        NUnitAssert.That(collection, NUnitFramework.Has.Exactly(1).Items);
        return collection.Single();
    }

    public static T Single<T>([NotNull] IEnumerable<T>? collection, T expected) where T : notnull
    {
        var item = Single(collection);
        Equal(expected, item);
        return item;
    }

    public static T Single<T>([NotNull] IEnumerable<T>? collection, Func<T, bool> predicate) where T : notnull
    {
        if (collection is null)
        {
            NUnitAssert.Fail("Expected collection with exactly one matching item.");
            throw new InvalidOperationException("Unreachable");
        }

        var matches = collection.Where(predicate).ToList();
        NUnitAssert.That(matches, NUnitFramework.Has.Exactly(1).Items);
        return matches[0];
    }

    // xUnit Assert.Superset(subset, superset) — subset must be contained in superset.
    public static void Superset<T>(IEnumerable<T> subset, IEnumerable<T> superset)
    {
        var supersetSet = superset as ISet<T> ?? superset.ToHashSet();
        NUnitAssert.That(subset.All(item => supersetSet.Contains(item)), NUnitFramework.Is.True);
    }

    public static void All<T>(IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }

    public static void Collection<T>(IEnumerable<T> collection, params Action<T>[] elementInspectors)
    {
        var items = collection.ToList();
        NUnitAssert.That(items, NUnitFramework.Has.Count.EqualTo(elementInspectors.Length));

        for (var i = 0; i < elementInspectors.Length; i++)
        {
            elementInspectors[i](items[i]);
        }
    }

    public static void SkipUnless(bool condition, string reason) => SkipWhen(!condition, reason);

    public static void DoesNotMatch(string pattern, string? actualString)
    {
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null);
        NUnitAssert.That(actualString!, NUnitFramework.Does.Not.Match(pattern));
    }

    public static void DoesNotMatch(Regex regex, string? actualString)
    {
        NUnitAssert.That(actualString, NUnitFramework.Is.Not.Null);
        NUnitAssert.That(actualString!, NUnitFramework.Does.Not.Match(regex));
    }

    public static void Skip(string reason) => NUnitAssert.Ignore(reason);

    public static void SkipWhen(bool condition, string reason)
    {
        if (condition)
        {
            NUnitAssert.Ignore(reason);
        }
    }

    public static T Throws<T>(Action testCode) where T : Exception
    {
        var exception = NUnitAssert.Throws<T>(() => testCode());
        return exception!;
    }

    public static T Throws<T>(string paramName, Action testCode) where T : ArgumentException
    {
        var exception = NUnitAssert.Throws<T>(() => testCode())!;
        NUnitAssert.That(exception.ParamName, NUnitFramework.Is.EqualTo(paramName));
        return exception;
    }

    public static T ThrowsAny<T>(Action testCode) where T : Exception
    {
        try
        {
            testCode();
        }
        catch (T exception)
        {
            return exception;
        }
        catch (Exception exception) when (exception is T)
        {
            return (T)exception;
        }

        NUnitAssert.Fail($"Expected any exception of type {typeof(T).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    public static async Task<T> ThrowsAsync<T>(Func<Task> testCode) where T : Exception
    {
        try
        {
            await testCode();
        }
        catch (T exception)
        {
            return exception;
        }

        NUnitAssert.Fail($"Expected exception of type {typeof(T).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    public static async Task<T> ThrowsAsync<T>(string paramName, Func<Task> testCode) where T : ArgumentException
    {
        var exception = await ThrowsAsync<T>(testCode);
        NUnitAssert.That(exception.ParamName, NUnitFramework.Is.EqualTo(paramName));
        return exception;
    }

    public static async Task<TActual> ThrowsAnyAsync<TActual>(Func<Task> testCode) where TActual : Exception
    {
        try
        {
            await testCode();
        }
        catch (TActual exception)
        {
            return exception;
        }
        catch (Exception exception) when (exception is TActual)
        {
            return (TActual)exception;
        }

        NUnitAssert.Fail($"Expected any exception of type {typeof(TActual).Name}.");
        throw new InvalidOperationException("Unreachable");
    }

    public static void Fail(string? message = null) => NUnitAssert.Fail(message ?? string.Empty);
}
