export async function ReleaseNotesServer(): Promise<{ date: string; name: string; notes: string }[]> {
    const API_URL = "https://oothkqo6lvnvbh7346hbxyduau0ghgsy.lambda-url.us-east-1.on.aws/";

    try {
        const response = await fetch(API_URL);
        return await response.json();
    } catch (error) {
        console.error("Error fetching releases:", error);
        return [];
    }
}
