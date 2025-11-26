import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import {useEffect, useState} from "react";
import {
    Server,
    SessionHandler,
    RestoreModal,
    ISavedGame,
    SaveModal,
    RestartConfirmDialog,
    VideoModal,
    ReleaseNotesModal,
    Mixpanel,
    DialogType,
    GameMenu,
    FunctionsMenu,
    Game,
    useGameContext
} from "@zork-ai/game-client-core";
import config from '../config.json';
import WelcomeDialog from "./modal/WelcomeModal.tsx";
import ZorkAboutMenu from "./menu/AboutMenu.tsx";

function App() {

    const [restartConfirmOpen, setRestartConfirmOpen] = useState<boolean>(false);
    const [restoreDialogOpen, setRestoreDialogOpen] = useState<boolean>(false);
    const [saveDialogOpen, setSaveDialogOpen] = useState<boolean>(false);
    const [availableSavedGames, setAvailableSavedGames] = useState<ISavedGame[]>([]);
    const [welcomeDialogOpen, setWelcomeDialogOpen] = useState<boolean>(false);
    const [videoDialogOpen, setVideoDialogOpen] = useState<boolean>(false);
    const [releaseNotesDialogOpen, setReleaseNotesDialogOpen] = useState<boolean>(false);

    const server = new Server(config.base_url);
    const sessionId = new SessionHandler();
    const queryClient = new QueryClient();

    const {dialogToOpen, setDialogToOpen, setRestartGame, setRestoreGameRequest, setSaveGameRequest, setDeleteGameRequest, copyGameTranscript} = useGameContext();

    useEffect(() => {
        if (!dialogToOpen) {
            return;
        }

        const handleDialog = async () => {
            switch (dialogToOpen) {
                case DialogType.Save:
                    await getSavedGames();
                    setSaveDialogOpen(true);
                    setDialogToOpen(undefined);
                    break;
                case DialogType.Video:
                    Mixpanel.track('Open Video Dialog', {});
                    setVideoDialogOpen(true);
                    setDialogToOpen(undefined);
                    break;
                case DialogType.Welcome:
                    Mixpanel.track('Open Welcome Dialog', {});
                    setWelcomeDialogOpen(true);
                    setDialogToOpen(undefined);
                    break;
                case DialogType.Restore:
                    await getSavedGames();
                    setRestoreDialogOpen(true);
                    setDialogToOpen(undefined);
                    break;
                case DialogType.Restart:
                    setRestartConfirmOpen(true);
                    setDialogToOpen(undefined);
                    break;
                case DialogType.ReleaseNotes:
                    Mixpanel.track('Open Release Notes Dialog', {});
                    setReleaseNotesDialogOpen(true);
                    setDialogToOpen(undefined);
                    break;
                default:
                    break;
            }
        };

        handleDialog().catch((error) => {
            console.error("App.tsx: Error handling dialog:", error);
        });

    }, [dialogToOpen]);

    const handleWatchVideo = () => {
        setWelcomeDialogOpen(false);
        setVideoDialogOpen(true);
    };

    async function getSavedGames() {
        const id = sessionId.getClientId();

        try {
            const savedGames = await server.getSavedGames(id);
            setAvailableSavedGames(savedGames);
        } catch (error) {
            console.error('App.tsx: Error fetching saved games:', error);
        }
    }

    return (

        <div
            className="bg-[url('./back2.png')] bg-repeat bg-[size:500px_500px] min-h-screen flex flex-col justify-between">
            <div className="flex-grow flex flex-col min-h-0 mt">
                <GameMenu
                    logoUrl="https://zorkai-assets.s3.amazonaws.com/Zork.webp"
                    logoAlt="Zork Logo"
                    title="Generative AI-Enhanced Zork I"
                >
                    <FunctionsMenu
                        onDialogOpen={setDialogToOpen}
                        onCopyTranscript={copyGameTranscript}
                    />
                    <ZorkAboutMenu />
                </GameMenu>

                <QueryClientProvider client={queryClient}>

                    <Game baseUrl={config.base_url} />

                    <RestartConfirmDialog
                        open={restartConfirmOpen}
                        setOpen={setRestartConfirmOpen}
                        onConfirm={() => {
                            setRestartGame(true);
                        }}
                    />

                    <RestoreModal
                        games={availableSavedGames}
                        open={restoreDialogOpen}
                        setOpen={setRestoreDialogOpen}
                        onRestoreGame={setRestoreGameRequest}
                        onDeleteGame={setDeleteGameRequest}
                    />

                    <SaveModal
                        games={availableSavedGames}
                        setOpen={setSaveDialogOpen}
                        open={saveDialogOpen}
                        onSaveGame={setSaveGameRequest}
                    />

                    <VideoModal
                        open={videoDialogOpen}
                        videoUrl="https://zorkai-assets.s3.amazonaws.com/zorkintro.mp4"
                        title="Welcome to Zork AI"
                        handleClose={() => {
                            setVideoDialogOpen(false);
                            Mixpanel.track('Close Video Dialog', {});
                        }}
                    />

                    <ReleaseNotesModal
                        title="Zork AI Release Notes"
                        handleClose={() => {
                            setReleaseNotesDialogOpen(false);
                            Mixpanel.track('Close Release Notes Dialog', {});
                        }}
                        open={releaseNotesDialogOpen}
                    />

                    <WelcomeDialog handleWatchVideo={handleWatchVideo} open={welcomeDialogOpen}
                                   handleClose={() => {
                                       setWelcomeDialogOpen(false);
                                       Mixpanel.track('Close Welcome Dialog', {});
                                   }}/>


                </QueryClientProvider>
            </div>

        </div>
    );
}

export default App;
