public class TestCaseConfig
{
    public long TimeLimit { get; set; }

    public long MemoryLimit { get; set; }

    public ValidatorType ValidatorType { get; set; }

    public TestCase[]? TestCases { get; set; }

    public double? Epsilon { get; set; }

    public string? ValidatorSourceCode { get; set; }

    public string? ValidatorLanguage { get; set; }
}

public class TestCase
{
    public string Input { get; set; }

    public string Output { get; set; }

    public int Score { get; set; }
}