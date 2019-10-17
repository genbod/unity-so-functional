using System;

using static F;

public struct Url
{
    private string Value { get; }
    private Url(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException($"{value} is not a vlid url");
        }

        Value = value;
    }

    private static bool IsValid(string url)
        => !String.IsNullOrEmpty(url) && (url.Contains("://") || url.Contains(":///"));

    public static Option<Url> Of(string url)
        => IsValid(url) ? Some(new Url(url)) : None;

    public override string ToString()
    {
        return Value.ToString();
    }
}
