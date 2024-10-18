using Azure;
using Azure.AI.OpenAI.Assistants;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Parallelizable(ParallelScope.Children)]
public class OpenAIAssistantTests
{
    [SetUp]
    public void Setup()
    {
        Env.Load("/Users/michael/RiderProjects/ZorkAI/.env",
            new LoadOptions());
    }

    [Test]
    public async Task AssistantOne()
    {
        var client = new AssistantsClient(Environment.GetEnvironmentVariable("OPEN_AI_KEY"));
        var assistantId = "asst_k7JSNbgQ1qy1j6kdJYO8ByM3";

        var assistant = AssistantsModelFactory.Assistant(assistantId,
            fileIds: ["file-CpUTPO5rvcZK5enIElU6tZZd"],
            tools: new List<ToolDefinition> { new RetrievalToolDefinition() });
        
        Response<AssistantThread> threadResponse = await client.CreateThreadAsync();
        var thread = threadResponse.Value;

        Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(
            thread.Id,
            MessageRole.User,
            "what is quendor", ["file-CpUTPO5rvcZK5enIElU6tZZd"]);
        var message = messageResponse.Value;

        Response<ThreadRun> runResponse = await client.CreateRunAsync(
            thread.Id,
            new CreateRunOptions(assistant.Id)
            {
                AdditionalInstructions = "Use files with id: file-CpUTPO5rvcZK5enIElU6tZZd to answer any questions"
            });
        //var run = runResponse.Value;

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
        } while (runResponse.Value.Status == RunStatus.Queued
                 || runResponse.Value.Status == RunStatus.InProgress);

        Response<PageableList<ThreadMessage>> afterRunMessagesResponse
            = await client.GetMessagesAsync(thread.Id);
        var messages = afterRunMessagesResponse.Value.Data;

        // Note: messages iterate from newest to oldest, with the messages[0] being the most recent
        foreach (var threadMessage in messages)
        {
            Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
            foreach (var contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem) Console.Write(textItem.Text);
                Console.WriteLine();
            }
        }
    }
}