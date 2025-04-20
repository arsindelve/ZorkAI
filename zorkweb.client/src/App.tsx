import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./menu/GameMenu.tsx";
import {useEffect, useState, useRef} from "react";
import Server from "./Server.ts";
import {SessionHandler} from "./SessionHandler.ts";
import RestoreModal from "./modal/RestoreModal.tsx";
import {ISavedGame} from "./model/SavedGame.ts";
import SaveModal from "./modal/SaveModal.tsx";
import ConfirmDialog from "./modal/ConfirmationDialog.tsx";
import {useGameContext} from "./GameContext.tsx";
import VideoDialog from "./modal/VideoModal.tsx";
import WelcomeDialog from "./modal/WelcomeModal.tsx";
import ReleaseNotesModal from "./modal/ReleaseNotesModal.tsx";
import {Mixpanel} from "./Mixpanel.ts";
import DialogType from "./model/DialogType.ts";
import {Popper, Paper, Typography, ClickAwayListener} from "@mui/material";

function App() {

    const [confirmOpen, setConfirmRestartOpen] = useState<boolean>(false);
    const [restoreDialogOpen, setRestoreDialogOpen] = useState<boolean>(false);
    const [saveDialogOpen, setSaveDialogOpen] = useState<boolean>(false);
    const [availableSavedGames, setAvailableSavedGames] = useState<ISavedGame[]>([]);
    const [welcomeDialogOpen, setWelcomeDialogOpen] = useState<boolean>(false);
    const [videoDialogOpen, setVideoDialogOpen] = useState<boolean>(false);
    const [releaseNotesDialogOpen, setReleaseNotesDialogOpen] = useState<boolean>(false);
    const [showInputPopper, setShowInputPopper] = useState<boolean>(false);
    const [welcomeShownOnLoad, setWelcomeShownOnLoad] = useState<boolean>(false);
    const [userHasTyped, setUserHasTyped] = useState<boolean>(false);
    const gameInputRef = useRef<HTMLElement | null>(null);

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
                case DialogType.Welcome: {
                    Mixpanel.track('Open Welcome Dialog', {});
                    setWelcomeDialogOpen(true);
                    // Check if this is the initial page load or from menu
                    const isFromPageLoad = sessionId.getSessionId()[1];
                    setWelcomeShownOnLoad(isFromPageLoad);
                    setDialogToOpen(undefined);
                    break;
                }
                case DialogType.Restore:
                    await getSavedGames();
                    setRestoreDialogOpen(true);
                    setDialogToOpen(undefined);
                    break;
                case DialogType.Restart:
                    setConfirmRestartOpen(true);
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

    const handleUserTyped = () => {
        setUserHasTyped(true);
        setShowInputPopper(false);
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
                    <Game
                        onUserTyped={handleUserTyped}
                    />

                    <ConfirmDialog
                        title="Restart Your Game? Are you sure?"
                        open={confirmOpen}
                        setOpen={setConfirmRestartOpen}
                        onConfirm={() => {
                            setRestartGame(true);
                        }}
                        message="Your game will be reset to the beginning. Are you sure you want to restart?"
                        confirmText="Restart"
                        cancelText="Cancel"
                        confirmColor="red"
                        dataTestId="restart-game-modal"
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

                                       // Show popper if welcome dialog was shown on page load
                                       if (welcomeShownOnLoad) {
                                           // Add a small delay to ensure DOM is updated
                                           setTimeout(() => {
                                               // Find the game input element
                                               const inputElement = document.querySelector('[data-testid="game-input"]');
                                               if (inputElement) {
                                                   gameInputRef.current = inputElement as HTMLElement;
                                                   // Only show popper if user hasn't typed anything yet
                                                   if (!userHasTyped) {
                                                       setShowInputPopper(true);
                                                       console.log("Showing popper", inputElement);
                                                   }
                                               } else {
                                                   console.log("Game input element not found");
                                               }
                                           }, 500); // 500ms delay
                                       }
                                   }}/>


                    {/* Input instruction popper */}
                    <Popper
                        open={showInputPopper}
                        anchorEl={gameInputRef.current}
                        placement="top"
                        style={{ zIndex: 9999 }}
                        modifiers={[
                            {
                                name: 'offset',
                                options: {
                                    offset: [0, 10],
                                },
                            },
                        ]}
                    >
                        <ClickAwayListener onClickAway={() => setShowInputPopper(false)}>
                            <Paper elevation={5} sx={{ 
                                p: 2, 
                                bgcolor: '#e3f2fd', 
                                maxWidth: 300, 
                                zIndex: 9999,
                                border: '2px solid #2196f3',
                                borderRadius: 2,
                                position: 'relative',
                                '&::after': {
                                    content: '""',
                                    position: 'absolute',
                                    bottom: '-10px',
                                    left: '50%',
                                    marginLeft: '-10px',
                                    borderWidth: '10px 10px 0',
                                    borderStyle: 'solid',
                                    borderColor: '#2196f3 transparent transparent',
                                }
                            }}>
                                <Typography variant="body1" fontWeight="bold">Type your instructions to Zork here, then press enter</Typography>
                            </Paper>
                        </ClickAwayListener>
                    </Popper>
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
