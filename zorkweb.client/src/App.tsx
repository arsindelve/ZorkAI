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
import {ISaveGameRequest} from "./model/SaveGameRequest.ts";
import ConfirmDialog from "./modal/ConfirmationDialog.tsx";
import {useGameContext} from "./GameContext.tsx";


function App() {

    const [confirmOpen, setConfirmRestartOpen] = useState<boolean>(false);
    const [forceMenuClose, setMenuForceClose] = useState<boolean>(false);
    const [gameSaved, setGameSaved] = useState<boolean>(false);
    const [restoreGameId, setRestoreGameId] = useState<string | undefined>(undefined);
    const [restoreDialogOpen, setRestoreDialogOpen] = useState<boolean>(false);
    const [saveDialogOpen, setSaveDialogOpen] = useState<boolean>(false);
    const [availableSavedGames, setAvailableSavedGames] = useState<ISavedGame[]>([]);
    const [serverText, setServerText] = useState<string>("");

    const server = new Server();
    const sessionId = new SessionHandler();
    const queryClient = new QueryClient();

    const {dialogToOpen, setDialogToOpen, setRestartGame} = useGameContext();

    useEffect(() => {

        if (!dialogToOpen)
            return;

        if (dialogToOpen === "Save") {
            setSaveDialogOpen(true);
            setDialogToOpen("");
        } else if (dialogToOpen === "Restore") {
            setRestoreDialogOpen(true);
            setDialogToOpen("");
        } else if (dialogToOpen === "Restart") {
            setConfirmRestartOpen(true);
            setDialogToOpen("");
        }

    }, [dialogToOpen]);

    async function restore(): Promise<void> {
        setMenuForceClose(false);
        await getSavedGames();
        setRestoreDialogOpen(true);
    }

    async function getSavedGames() {
        const id = sessionId.getClientId();
        const savedGames = await server.getSavedGames(id);
        setAvailableSavedGames(savedGames);
    }

    async function save(): Promise<void> {
        setMenuForceClose(false);
        await getSavedGames();
    }

    function handleRestoreModalClose(id: string | undefined): void {
        if (id)
            setRestoreGameId(id);
        setRestoreDialogOpen(false);
        setMenuForceClose(true);
    }

    async function handleSaveModalClose(request: ISaveGameRequest | undefined): Promise<void> {
        setGameSaved(false);
        if (request) {
            request.sessionId = sessionId.getSessionId()[0];
            request.clientId = sessionId.getClientId()
            setServerText(await server.saveGame(request));
        }
        setSaveDialogOpen(false);
        setMenuForceClose(true);
        setGameSaved(true);
    }

    return (

        <div
            className="bg-[url('https://zorkai-assets.s3.amazonaws.com/brick-wall-background-texture.jpg')] bg-repeat bg-[size:500px_500px] min-h-screen flex flex-col justify-between">
            <div className="flex-grow flex flex-col min-h-0 mt">
                <GameMenu forceClose={forceMenuClose} gameMethods={[restore, save]}/>

                <QueryClientProvider client={queryClient}>
                    <Game
                        serverText={serverText}
                        onRestoreDone={() => setRestoreGameId(undefined)}
                        restoreGameId={restoreGameId}
                        gaveSaved={gameSaved}
                        onRestartDone={() => {
                            setConfirmRestartOpen(false);
                            setMenuForceClose(true);
                        }}
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
                    />

                    <RestoreModal games={availableSavedGames} 
                                  open={restoreDialogOpen}
                                  handleClose={handleRestoreModalClose}/>

                    <SaveModal games={availableSavedGames} 
                               open={saveDialogOpen}
                               handleClose={handleSaveModalClose}/>
                    
                </QueryClientProvider>
            </div>

            <footer className="bg-gray-200 py-2">
                <p className="text-center text-sm text-black font-['Lato']">
                    <a target="_blank" href="https://github.com/arsindelve/ZorkAI">Created By Mike in Dallas. Check
                        out
                        the repository.</a>
                </p>
            </footer>
        </div>


    );
}

export default App;
