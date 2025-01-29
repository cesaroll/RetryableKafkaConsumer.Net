using System.Text;
using Confluent.Kafka;

namespace RetryableConsumer.Processor.Extensions;

public static class MessageHeaderExtensions
{
    private const string LocalRetryAttemptsCountHeader = "LOCAL_RETRY_ATTEMPTS_COUNT";
    private const string OverallRetryAttemptsCountHeader = "OVERALL_RETRY_ATTEMPTS_COUNT";
    
    public static void SetLocalRetryCountHeader<TKey, TValue>(this Message<TKey, TValue> message, int count)
        => message.SetHeader(LocalRetryAttemptsCountHeader, count.ToString());
    
    public static void SetOverallRetryCountHeader<TKey, TValue>(this Message<TKey, TValue> message, int count)
        => message.SetHeader(OverallRetryAttemptsCountHeader, count.ToString());
    
    public static int GetLocalRetryCountHeader<TKey, TValue>(this Message<TKey, TValue> message)
        => int.TryParse(message.GetHeader(LocalRetryAttemptsCountHeader), out var count) ? count : 0;
    
    public static int GetOverallRetryCountHeader<TKey, TValue>(this Message<TKey, TValue> message)
        => int.TryParse(message.GetHeader(OverallRetryAttemptsCountHeader), out var count) ? count : 0;
    
    private static void SetHeader<TKey, TValue>(this Message<TKey, TValue> message, string headerName, string headerValue)
    {
        message.Headers.Remove(headerName);
        message.Headers.Add(headerName, Encoding.UTF8.GetBytes(headerValue));
    }
    
    private static string GetHeader<TKey, TValue>(this Message<TKey, TValue> message, string headerName)
    {
        var header = message.Headers.FirstOrDefault(h => h.Key == headerName);
        return header == null ? string.Empty : Encoding.UTF8.GetString(header.GetValueBytes());
    }
}