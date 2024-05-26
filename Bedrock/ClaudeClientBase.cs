using System.Net;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Util;
using Newtonsoft.Json;

namespace Bedrock;

public abstract class ClaudeClientBase
{
    //private const string ClaudeModelId = "amazon.titan-text-premier-v1:0";
    private const string ClaudeModelId = "anthropic.claude-3-sonnet-20240229-v1:0";
    //private const string ClaudeModelId = "anthropic.claude-3-haiku-20240307-v1:0";
    protected const string AnthropicVersion = "bedrock-2023-05-31";

    protected async Task<string?> GenerateResponse(string jsonString)
    {
        AmazonBedrockRuntimeClient client = new(RegionEndpoint.USEast1);

        var response = await client.InvokeModelAsync(new InvokeModelRequest
        {
            ModelId = ClaudeModelId,
            Body = AWSSDKUtils.GenerateMemoryStreamFromString(jsonString),
            ContentType = "application/json",
            Accept = "application/json"
        });

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            using var streamReader = new StreamReader(response.Body);
            var content = await streamReader.ReadToEndAsync();
            var responseContent = JsonConvert.DeserializeObject<dynamic>(content) ??
                                  throw new InvalidOperationException();

            return responseContent.content[0].text;
        }

        return null;
    }
}