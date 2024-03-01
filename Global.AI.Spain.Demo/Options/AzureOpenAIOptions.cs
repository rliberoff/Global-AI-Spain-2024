using System.ComponentModel.DataAnnotations;

using Azure.AI.OpenAI;

namespace Global.AI.Spain.Demo.Options;

public sealed class AzureOpenAIOptions
{
    [Required]
    public required OpenAIClientOptions.ServiceVersion ServiceVersion { get; init; } = OpenAIClientOptions.ServiceVersion.V2023_12_01_Preview;

    [Required]
    public required string ChatModelDeploymentName { get; init; }

    [Required]
    public required Uri Endpoint { get; init; }

    [Required]
    public required string Key { get; init; }
}
