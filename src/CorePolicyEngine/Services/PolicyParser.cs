using System.Xml.Linq;

namespace CorePolicyEngine.Services;

public interface IPolicyParser
{
    PolicyDocument Parse(string admxXml);
}

public sealed record PolicyDocument(string Name, string RawAdmx);

public sealed class PolicyParser : IPolicyParser
{
    public PolicyDocument Parse(string admxXml)
    {
        try
        {
            var x = XDocument.Parse(admxXml);
            var name = x.Root?.Attribute("name")?.Value ?? "Unknown";
            return new PolicyDocument(name, admxXml);
        }
        catch
        {
            return new PolicyDocument("Invalid", admxXml);
        }
    }
}
