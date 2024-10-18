from openai import OpenAI

client = OpenAI()

assistant = client.beta.assistants.retrieve("asst_k7JSNbgQ1qy1j6kdJYO8ByM3")
thread = client.beta.threads.create(
    messages=[
        {
            "role": "user",
            "content": "kill the troll",
            "attachments": [
                {"file_id": "file-CpUTPO5rvcZK5enIElU6tZZd", "tools": [{"type": "file_search"}]}
            ],
        }
    ]
)

run = client.beta.threads.runs.create_and_poll(
    thread_id=thread.id,
    assistant_id=assistant.id,
)

if run.status == 'completed':
    messages = client.beta.threads.messages.list(
        thread_id=thread.id
    )
    print(messages)
else:
    print(run.status)
