﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Globalization;

/// <summary>
/// Extension methods for the string data type
/// </summary>
public static class StringExtensions
{
	private static readonly Regex segmentRegex = new Regex(@"[^A-Za-z0-9\-_~]{1}");
	private static readonly Regex dashRegex = new Regex(@"[\-\-]+");
	private static readonly Regex spaceRegex = new Regex(@"\s+");

    /// <summary>
    /// Extension method for html decoding a string
    /// </summary>
    /// <param name="s">The string to html decode</param>
    /// <returns>A html decoded string</returns>
    public static string HtmlDecode(this string s)
    {
        return HttpUtility.HtmlDecode(s);
    }

    /// <summary>
    /// Extension method for stripping all html from a string
    /// </summary>
    /// <param name="s">The string to remove html from</param>
    /// <returns>A string without html tags</returns>
    public static string StripHtml(this string s)
    {
        return Regex.Replace(s, @"(\<[^\>]+\>)", string.Empty);
    }

    /// <summary>
    /// Extension method for adding a parameter to a query string
    /// </summary>
    /// <param name="s">The string to add parameter to</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <returns>A string with the added parameter</returns>
    public static string AddQueryStringParameter(this string s, string name, string value)
    {
        var splitOnQuestionMark = s.Split(new[] { '?' });
        var leftPart = splitOnQuestionMark[0];
        var queryString = splitOnQuestionMark.Length > 1 ? splitOnQuestionMark[1] : string.Empty;
        var q = HttpUtility.ParseQueryString(queryString);
        q[name] = HttpUtility.UrlEncode(value);
        return string.Format("{0}?{1}",
            leftPart,
            string.Join("&", q.AllKeys.Select(k => string.Format("{0}={1}", k, q.Get(k)))));
    }

    /// <summary>
    /// Extension method for converting the first character in a string to upper case
    /// </summary>
    /// <param name="s">The string to convert first character to upper case in</param>
    /// <returns>A string with the first character converted to upper case</returns>
    public static string ToFirstUpper(this string s)
    {
        if (!string.IsNullOrWhiteSpace(s))
            return char.ToUpper(s[0]) + s.Substring(1);
        return s;
    }

    /// <summary>
    /// Extension method for checking whether a string is null or contains whitespaces only
    /// </summary>
    /// <param name="s">The string to check</param>
    /// <returns>
    /// <c>true</c> if the string is null or contains whitespace(s) only; otherwise <c>false</c>
    /// </returns>
    public static bool IsNullOrWhiteSpace(this string s)
    {
        return System.String.IsNullOrWhiteSpace(s);
    }

    /// <summary>
    /// Extension method to reverse a string
    /// </summary>
    /// <param name="s">The string to be reversed</param>
    /// <returns>The reversed string</returns>
    public static string Reverse(this string s)
    {
        if (s.IsNullOrWhiteSpace() || (s.Length == 1))
            return s;

        var chars = s.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Extension method for checking if a string is a valid url
    /// </summary>
    /// <param name="s">The string to check</param>
    /// <returns>
    /// True if the string is a valid url; otherwise false
    /// </returns>
    public static bool IsValidUrl(this string s)
    {
        Regex rx = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        return rx.IsMatch(s);
    }

	/// <summary>
	/// It looks like you are trying to create a slug.
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static string ToSlug(this string s)
	{
		if (s == null)
			return null;

		s = s.Trim();
		s = spaceRegex.Replace(s, "-")
			.Replace("&", "-")
			.Replace("/", "-")
			.Replace(".", "-")
			.Replace("$", "s")
			.Replace("’", string.Empty)
			.Replace("'", string.Empty);

		string formD = s.Normalize(NormalizationForm.FormD);
		StringBuilder sb = new StringBuilder();

		for (int i = 0; i < formD.Length; i++)
		{
			UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(formD[i]);
			if (uc != UnicodeCategory.NonSpacingMark)
			{
				sb.Append(formD[i]);
			}
		}

		s = dashRegex.Replace(segmentRegex.Replace(sb.ToString(), string.Empty), "-");
		if (s.EndsWith("-"))
			s = s.Substring(0, s.Length - 1);

		return s.ToLower();
	}

	/// <summary>
	/// Truncates a string to specified length
	/// </summary>
	/// <param name="text"></param>
	/// <param name="maxLength">the total allowed length of the string INCLUDING the suffix</param>
	/// <param name="cutInWhitespace">whether only cut in whitespace (if possible)</param>
	/// <param name="suffix">what suffix to use, defaults to '...'</param>
	/// <returns>the truncated string</returns>
	public static string Truncate(this string text, int maxLength, bool cutInWhitespace = false, string suffix = "...")
	{
		if (string.IsNullOrWhiteSpace(text)) return text;
		if (text.Length <= maxLength) return text;

		if (suffix.Length >= maxLength)
			throw new ArgumentException("Your maxLength is less or equal to the length of your suffix. That would look weird!");

		var trunc = text.Substring(0, maxLength - suffix.Length);
		if (cutInWhitespace)
		{
			var lastWhitespace = trunc.LastIndexOf(' ');
			if (lastWhitespace > 1) 
			{ 
				trunc = trunc.Substring(0, lastWhitespace);
			}
		}

		return string.Concat(trunc, suffix);
	}

	/// <summary>
	/// Tries to split the string with the provided token and change 
	/// the type of the values to the type provided in <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="tokenString"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public static bool TryParseTokenSeparatedValues<T>(this string tokenString, out IEnumerable<T> result, string token = ",")
	{
		// Create a list to store the values.
		var parsedValues = new List<T>();
		// and set it to be the out list.
		result = parsedValues;

		try
		{
			tokenString.Split(token.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
				.Each((value) => 
				{
					T parsedValue = (T)Convert.ChangeType(value.Trim(), typeof(T));
					parsedValues.Add(parsedValue);
				});
		}

		// Catch all parse errors since this is a "Try Parse" method
		catch { return false; }

		// All parsing was successfull, return true.
		return true;
	}
}