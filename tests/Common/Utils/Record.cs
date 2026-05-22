namespace ModelContextProtocol.Tests.Utils;

public static class Record
{
    public static Exception? Exception(Action testCode)
    {
        try
        {
            testCode();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public static async Task<Exception?> ExceptionAsync(Func<Task> testCode)
    {
        try
        {
            await testCode();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
