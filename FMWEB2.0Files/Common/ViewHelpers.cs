using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

internal static class ViewHelpers
{
    public static JsonSerializerSettings CamelCase
    {
        get
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}