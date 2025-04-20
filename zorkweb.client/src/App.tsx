import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./menu/GameMenu.tsx";
import {useEffect, useState} from "react";
import Server from "./Server.ts";
import {SessionHandler} from "./SessionHandler.ts";
import RestoreModal from "./modal/RestoreModal.tsx";
import {ISavedGame} from "./model/SavedGame.ts";
import SaveModal from "./modal/SaveModal.tsx";
import RestartConfirmDialog from "./modal/RestartConfirmDialog.tsx";
import {useGameContext} from "./GameContext.tsx";
import VideoDialog from "./modal/VideoModal.tsx";
import WelcomeDialog from "./modal/WelcomeModal.tsx";
import ReleaseNotesModal from "./modal/ReleaseNotesModal.tsx";
import {Mixpanel} from "./Mixpanel.ts";
import DialogType from "./model/DialogType.ts";

function App() {

    const [restartConfirmOpen, setRestartConfirmOpen] = useState<boolean>(false);
    const [restoreDialogOpen, setRestoreDialogOpen] = useState<boolean>(false);
    const [saveDialogOpen, setSaveDialogOpen] = useState<boolean>(false);
    const [availableSavedGames, setAvailableSavedGames] = useState<ISavedGame[]>([]);
    const [welcomeDialogOpen, setWelcomeDialogOpen] = useState<boolean>(false);
    const [videoDialogOpen, setVideoDialogOpen] = useState<boolean>(false);
    const [releaseNotesDialogOpen, setReleaseNotesDialogOpen] = useState<boolean>(false);

    const server = new Server();
    const sessionId = new SessionHandler();
    const queryClient = new QueryClient();

    const {dialogToOpen, setDialogToOpen, setRestartGame} = useGameContext();

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
            className="bg-[url('https://zorkai-assets.s3.amazonaws.com/brick-wall-background-texture.jpg')] bg-repeat bg-[size:500px_500px] min-h-screen flex flex-col justify-between">
            <div className="flex-grow flex flex-col min-h-0 mt">
                <GameMenu/>

                <QueryClientProvider client={queryClient}>

                    <Game />

                    <RestartConfirmDialog
                        open={restartConfirmOpen}
                        setOpen={setRestartConfirmOpen}
                        onConfirm={() => {
                            setRestartGame(true);
                        }}
                    />

                    <RestoreModal games={availableSavedGames}
                                  open={restoreDialogOpen}
                                  setOpen={setRestoreDialogOpen}
                    />

                    <SaveModal games={availableSavedGames}
                               setOpen={setSaveDialogOpen}
                               open={saveDialogOpen}
                    />

                    <VideoDialog open={videoDialogOpen}
                                 handleClose={() => {
                                     setVideoDialogOpen(false);
                                     Mixpanel.track('Close Video Dialog', {});
                                 }}/>

                    <ReleaseNotesModal handleClose={() => {
                        setReleaseNotesDialogOpen(false);
                        Mixpanel.track('Close Release Notes Dialog', {});
                    }} open={releaseNotesDialogOpen}/>

                    <WelcomeDialog handleWatchVideo={handleWatchVideo} open={welcomeDialogOpen}
                                   handleClose={() => {
                                       setWelcomeDialogOpen(false);
                                       Mixpanel.track('Close Welcome Dialog', {});
                                   }}/>


                </QueryClientProvider>
            </div>

            <footer className="bg-gray-200 py-2">
                <p className="text-center text-sm text-black font-['Lato']">
                    <a target="_blank" href="https://github.com/arsindelve/ZorkAI">Created By Mike in Dallas. Check
                        out
                        the repository. hello@newzork.ai</a>
                </p>
            </footer>
        </div>
    );
}

export default App;
