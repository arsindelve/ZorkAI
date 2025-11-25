interface GitHubRelease {
    published_at?: string;
    created_at: string;
    name?: string;
    tag_name: string;
    body_html?: string;
}

export async function ReleaseNotesServer(): Promise<{ date: string; name: string; notes: string }[]> {
    // Fetch directly from GitHub Releases API
    const GITHUB_API_URL = "https://api.github.com/repos/arsindelve/ZorkAI/releases";

    try {
        const response = await fetch(GITHUB_API_URL, {
            headers: {
                // Request HTML-rendered body instead of Markdown
                'Accept': 'application/vnd.github.v3.html+json'
            }
        });

        if (!response.ok) {
            throw new Error(`GitHub API error: ${response.status}`);
        }

        const releases: GitHubRelease[] = await response.json();

        // Transform GitHub release format to expected format
        // Filter out releases with no body text
        return releases
            .filter((release: GitHubRelease) => {
                const body = release.body_html || '';
                return body.trim().length > 0;
            })
            .map((release: GitHubRelease) => ({
                date: release.published_at || release.created_at,
                name: release.name || release.tag_name,
                notes: release.body_html || ''
            }));
    } catch (error) {
        console.error("Error fetching releases:", error);
        return [];
    }
}
