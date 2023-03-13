using System.Net;
using System.Text.Json;
using Xunit;

namespace AzureIPNetworks.Tests;

public class IPNetworkJsonConverterTests
{
    [Fact]
    public void IPNetworkConverter_Works()
    {
        var src_json = "{\"Network\":\"51.4.32.0/19\"}";
        var options = new JsonSerializerOptions { };
        options.Converters.Add(new IPNetworkJsonConverter());
        var model = JsonSerializer.Deserialize<TestModel>(src_json, options);
        var dst_json = JsonSerializer.Serialize(model, options);
        Assert.Equal(src_json, dst_json);
    }

    class TestModel
    {
        public IPNetwork? Network { get; set; }
    }
}
