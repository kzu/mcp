using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NUnit.Framework;

namespace ModelContextProtocol.Tests.Utils;

public static class XunitAssert
{
    public static void Equal(object? expected, object? actual)
    {
        if (expected is string)
        {
            NUnit.Framework.Assert.That(actual?.ToString(), Is.EqualTo(expected));
            return;
        }

        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (expected is ReadOnlyMemory<byte> expectedMemory && actual is ReadOnlyMemory<byte> actualMemory)
        {
            NUnit.Framework.Assert.That(actualMemory.Span.SequenceEqual(expectedMemory.Span), Is.True);
            return;
        }

        if (expected is byte[] expectedBytes && actual is byte[] actualBytes)
        {
            NUnit.Framework.Assert.That(actualBytes, Is.EqualTo(expectedBytes).AsCollection);
            return;
        }

        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
    }
    public static void Equal<T>(T expected, T actual, string message) => NUnit.Framework.Assert.That(actual, Is.EqualTo(expected), message);
    public static void NotEqual(object? expected, object? actual) => NUnit.Framework.Assert.That(actual, Is.Not.EqualTo(expected));
    public static void NotEqual<T>(T expected, T actual) => NUnit.Framework.Assert.That(actual, Is.Not.EqualTo(expected));
    public static void True(bool condition) => NUnit.Framework.Assert.That(condition, Is.True);
    public static void True(bool? condition) => NUnit.Framework.Assert.That(condition is true, Is.True);
    public static void True(bool condition, string message) => NUnit.Framework.Assert.That(condition, Is.True, message);
    public static void True(bool? condition, string message) => NUnit.Framework.Assert.That(condition is true, Is.True, message);
    public static void False(bool condition) => NUnit.Framework.Assert.That(condition, Is.False);
    public static void False(bool? condition) => NUnit.Framework.Assert.That(condition is false, Is.True);
    public static void False(bool condition, string message) => NUnit.Framework.Assert.That(condition, Is.False, message);
    public static void False(bool? condition, string message) => NUnit.Framework.Assert.That(condition is false, Is.True, message);
    public static void Null(object? value) => NUnit.Framework.Assert.That(value, Is.Null);
    public static void NotNull([NotNull] object? value)
    {
        NUnit.Framework.Assert.That(value, Is.Not.Null);
        ArgumentNullException.ThrowIfNull(value);
    }
    public static void Empty(IEnumerable collection) => NUnit.Framework.Assert.That(collection, Is.Empty);
    public static void NotEmpty(IEnumerable collection) => NUnit.Framework.Assert.That(collection, Is.Not.Empty);
    public static T Single<T>(IEnumerable<T> collection)
    {
        var items = collection.ToList();
        NUnit.Framework.Assert.That(items, Has.Count.EqualTo(1));
        return items[0];
    }

    public static T Single<T>(IEnumerable<T> collection, Func<T, bool> predicate)
    {
        var items = collection.Where(predicate).ToList();
        NUnit.Framework.Assert.That(items, Has.Count.EqualTo(1));
        return items[0];
    }

    public static T Single<T>(IEnumerable<T> collection, T expected)
    {
        var item = Single(collection);
        Equal(expected, item);
        return item;
    }

    public static void Contains<T>(T expected, IEnumerable<T> collection) => NUnit.Framework.Assert.That(collection, Does.Contain(expected));
    public static void Contains(string expectedSubstring, string? actualString) => NUnit.Framework.Assert.That(actualString, Does.Contain(expectedSubstring));
    public static void Contains<T>(IEnumerable<T> collection, Func<T, bool> predicate) => NUnit.Framework.Assert.That(collection.Any(predicate), Is.True);
    public static void Contains(string expectedSubstring, string? actualString, StringComparison comparisonType) => NUnit.Framework.Assert.That(actualString?.Contains(expectedSubstring, comparisonType), Is.True);
    public static void DoesNotContain<T>(T expected, IEnumerable<T> collection) => NUnit.Framework.Assert.That(collection, Does.Not.Contain(expected));
    public static void DoesNotContain(string expectedSubstring, string? actualString) => NUnit.Framework.Assert.That(actualString, Does.Not.Contain(expectedSubstring));
    public static void DoesNotContain(string expectedSubstring, string? actualString, StringComparison comparisonType) => NUnit.Framework.Assert.That(actualString?.Contains(expectedSubstring, comparisonType), Is.False);
    public static void DoesNotContain<T>(IEnumerable<T> collection, Func<T, bool> predicate) => NUnit.Framework.Assert.That(collection.Any(predicate), Is.False);
    public static T Throws<T>(TestDelegate code) where T : Exception => NUnit.Framework.Assert.Throws<T>(code)!;

    public static T Throws<T>(string? paramName, TestDelegate code) where T : ArgumentException
    {
        var exception = Throws<T>(code);
        Equal(paramName, exception.ParamName);
        return exception;
    }
    public static T ThrowsAny<T>(TestDelegate code) where T : Exception
    {
        try
        {
            code();
        }
        catch (Exception ex) when (ex is T match)
        {
            return match;
        }

        NUnit.Framework.Assert.Fail($"Expected exception assignable to {typeof(T).FullName}.");
        throw new UnreachableException();
    }

    public static Task<T> ThrowsAsync<T>(AsyncTestDelegate code) where T : Exception
    {
        var exception = NUnit.Framework.Assert.ThrowsAsync<T>(code);
        return Task.FromResult(exception!);
    }

    public static async Task<T> ThrowsAsync<T>(Delegate code) where T : Exception
    {
        try
        {
            var result = code.DynamicInvoke();
            if (result is Task task)
            {
                await task;
            }
            else
            {
                NUnit.Framework.Assert.Fail("Expected an async delegate returning Task.");
            }
        }
        catch (Exception ex) when ((ex as TargetInvocationException)?.InnerException is T match)
        {
            return match;
        }
        catch (Exception ex) when (ex is T match)
        {
            return match;
        }

        NUnit.Framework.Assert.Fail($"Expected exception assignable to {typeof(T).FullName}.");
        throw new UnreachableException();
    }

    public static async Task<T> ThrowsAsync<T>(string? paramName, AsyncTestDelegate code) where T : ArgumentException
    {
        var exception = await ThrowsAsync<T>(code);
        Equal(paramName, exception.ParamName);
        return exception;
    }
    public static async Task<T> ThrowsAnyAsync<T>(AsyncTestDelegate code) where T : Exception
    {
        try
        {
            await code();
        }
        catch (Exception ex) when (ex is T match)
        {
            return match;
        }

        NUnit.Framework.Assert.Fail($"Expected exception assignable to {typeof(T).FullName}.");
        throw new UnreachableException();
    }

    public static T IsType<T>([NotNull] object? value)
    {
        NUnit.Framework.Assert.That(value, Is.TypeOf<T>());
        ArgumentNullException.ThrowIfNull(value);
        return (T)value;
    }

    public static T IsAssignableFrom<T>([NotNull] object? value)
    {
        NUnit.Framework.Assert.That(value, Is.InstanceOf<T>());
        ArgumentNullException.ThrowIfNull(value);
        return (T)value;
    }

    public static void Same(object? expected, object? actual) => NUnit.Framework.Assert.That(actual, Is.SameAs(expected));
    public static void NotSame(object? expected, object? actual) => NUnit.Framework.Assert.That(actual, Is.Not.SameAs(expected));
    public static void StartsWith(string expectedStart, string? actualString) => NUnit.Framework.Assert.That(actualString, Does.StartWith(expectedStart));
    public static void EndsWith(string expectedEnd, string? actualString) => NUnit.Framework.Assert.That(actualString, Does.EndWith(expectedEnd));
    public static void Matches(string pattern, string? actualString) => NUnit.Framework.Assert.That(actualString, Does.Match(pattern));
    public static void DoesNotMatch(string pattern, string? actualString) => NUnit.Framework.Assert.That(actualString, Does.Not.Match(pattern));
    public static void InRange<T>(T actual, T low, T high) where T : IComparable => NUnit.Framework.Assert.That(actual, Is.InRange(low, high));
    public static void Superset<T>(IEnumerable<T> subset, IEnumerable<T> superset) => NUnit.Framework.Assert.That(superset, Is.SupersetOf(subset));
    public static void Skip(string reason) => NUnit.Framework.Assert.Ignore(reason);
    public static void SkipWhen(bool condition, string reason)
    {
        if (condition)
        {
            NUnit.Framework.Assert.Ignore(reason);
        }
    }

    public static void SkipUnless(bool condition, string reason)
    {
        if (!condition)
        {
            NUnit.Framework.Assert.Ignore(reason);
        }
    }

    public static void Fail(string? reason = null) => throw new Exception(reason ?? string.Empty);
    public static void All<T>(IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
    }

    public static void Collection<T>(IEnumerable<T> collection, params Action<T>[] inspectors)
    {
        var items = collection.ToList();
        NUnit.Framework.Assert.That(items, Has.Count.EqualTo(inspectors.Length));
        for (var i = 0; i < inspectors.Length; i++)
        {
            inspectors[i](items[i]);
        }
    }
}
