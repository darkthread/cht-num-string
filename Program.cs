using System.Text;
using System.Text.RegularExpressions;

var numSets = new[] {
    "0000", "0001", "0010", "0012",
    "0100", "0102", "0310", "0123",
    "1000", "1002", "1020", "1023",
    "1200", "1203", "1230", "1234"
};
// 列出所有可能的組合
var count = numSets.Length;
var idxs = new int[4];
var lv = idxs.Length - 1;
while (true)
{
    var sb = new StringBuilder();
    for (var i = 0; i < idxs.Length; i++)
    {
        sb.Append(numSets[idxs[i]]);
    }
    long n = long.Parse(sb.ToString());
    var cht = ChtNumConverter.ToChtNum(n);
    var restored = ChtNumConverter.ParseChtNum(cht);
    Console.WriteLine($"{n, 16:n0} {restored, 16:n0} " + 
        $"\x1b[{(n == restored ? "32mPASS" : "31mFAIL")}\x1b[0m {cht} ");
    if (n != restored)
        throw new ApplicationException($"數字轉換錯誤 {n} vs {restored}");
    if (idxs.All(o => o == count - 1)) break;
    idxs[lv]++;
    while (idxs[lv] == count)
    {
        idxs[lv] = 0;
        lv--;
        if (lv < 0) break;
        idxs[lv]++;
    }
    lv = idxs.Length - 1;
}

public class ChtNumConverter
{
    public static string ChtNums = "零一二三四五六七八九";
    public static Dictionary<string, long> ChtUnits = new Dictionary<string, long>{
            {"十", 10},
            {"百", 100},
            {"千", 1000},
            {"萬", 10000},
            {"億", 100000000},
            {"兆", 1000000000000}
        };
    public static long ParseChtNum(string chtNumString)
    {
        var isNegative = false;
        if (chtNumString.StartsWith("負"))
        {
            chtNumString = chtNumString.Substring(1);
            isNegative = true;
        }
        long num = 0;
        Func<string, long> Parse4Digits = (s) =>
        {
            long lastDigit = 0;
            long subNum = 0;
            foreach (var rawChar in s)
            {
                var c = rawChar.ToString().Replace("〇", "零");
                if (ChtNums.Contains(c))
                {
                    lastDigit = (long)ChtNums.IndexOf(c);
                }
                else if (ChtUnits.ContainsKey(c))
                {
                    if (c == "十" && lastDigit == 0) lastDigit = 1;
                    long unit = ChtUnits[c];
                    subNum += lastDigit * unit;
                    lastDigit = 0;
                }
                else
                {
                    throw new ArgumentException($"包含無法解析的中文數字：{c}");
                }
            }
            subNum += lastDigit;
            return subNum;
        };
        foreach (var splitUnit in "兆億萬".ToArray())
        {
            var pos = chtNumString.IndexOf(splitUnit);
            if (pos == -1) continue;
            var subNumString = chtNumString.Substring(0, pos);
            chtNumString = chtNumString.Substring(pos + 1);
            num += Parse4Digits(subNumString) * ChtUnits[splitUnit.ToString()];
        }
        num += Parse4Digits(chtNumString);
        return isNegative ? -num : num;
    }

    public static string ToChtNum(long n)
    {
        var negtive = n < 0;
        if (negtive) n = -n;
        if (n >= 10000 * ChtUnits["兆"])
            throw new ArgumentException("數字超出可轉換範圍");
        var unitChars = "千百十".ToArray();
        Func<long, string> Conv4Digits = (subNum) =>
        {
            var sb = new StringBuilder();
            foreach (var c in unitChars)
            {
                if (subNum >= ChtUnits[c.ToString()])
                {
                    var digit = subNum / ChtUnits[c.ToString()];
                    subNum = subNum % ChtUnits[c.ToString()];
                    sb.Append($"{ChtNums[(int)digit]}{c}");
                }
                else sb.Append("零");
            }
            sb.Append(ChtNums[(int)subNum]);
            return sb.ToString();
        };
        var numString = new StringBuilder();
        var forceRun = false;
        foreach (var splitUnit in "兆億萬".ToArray())
        {
            var unit = ChtUnits[splitUnit.ToString()];
            if (n < unit)
            {
                if (forceRun) numString.Append("零");
                continue;
            }
            forceRun = true;
            var subNum = n / unit;
            n = n % unit;
            if (subNum > 0)
                numString.Append(Conv4Digits(subNum).TrimEnd('零') + splitUnit);
            else numString.Append("零");
        }
        numString.Append(Conv4Digits(n));
        var t = Regex.Replace(numString.ToString(), "[零]+", "零");
        if (t.Length > 1) t = t.Trim('零');
        t = Regex.Replace(t, "^一十", "十");
        return (negtive ? "負" : string.Empty) + t;
    }
}