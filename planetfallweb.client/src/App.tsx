import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./menu/GameMenu.tsx";
import {useState} from "react";
import Server from "./Server.ts";
import {SessionHandler} from "./SessionHandler.ts";
import RestoreModal from "./modal/RestoreModal.tsx";
import {ISavedGame} from "./model/SavedGame.ts";
import SaveModal from "./modal/SaveModal.tsx";
import {ISaveGameRequest} from "./model/SaveGameRequest.ts";
import ConfirmDialog from "./modal/ConfirmationDialog.tsx";


function App() {

    const [confirmOpen, setConfirmRestartOpen] = useState<boolean>(false);
    const [forceMenuClose, setMenuForceClose] = useState<boolean>(false);
    const [gameSaved, setGameSaved] = useState<boolean>(false);
    const [restoreGameId, setRestoreGameId] = useState<string | undefined>(undefined);
    const [restoreDialogOpen, setRestoreDialogOpen] = useState<boolean>(false);
    const [saveDialogOpen, setSaveDialogOpen] = useState<boolean>(false);
    const [availableSavedGames, setAvailableSavedGames] = useState<ISavedGame[]>([]);
    const [isRestarting, setIsRestarting] = useState<boolean>(false);
    const [serverText, setServerText] = useState<string>("");

    const server = new Server();
    const sessionId = new SessionHandler();
    const queryClient = new QueryClient();

    function restart() {
        setIsRestarting(false);
        setConfirmRestartOpen(true);
    }

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
        setSaveDialogOpen(true);
    }

    function handleRestartGameConfirmClose() {
        setIsRestarting(true);
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
            request.clientId = sessionId.getClientId();
            setServerText(await server.saveGame(request));
        }
        setSaveDialogOpen(false);
        setMenuForceClose(true);
        setGameSaved(true);
    }

    async function handleDeleteGame(game: ISavedGame): Promise<void> {
        try {
            await server.deleteSavedGame(game.id!, sessionId.getClientId());
            // Refresh the saved games list
            await getSavedGames();
        } catch (error) {
            console.error('Error deleting saved game:', error);
            // Could add error handling here
        }
    }

    return (
        <div
            className="bg-[url('https://planetfallai-assets.s3.amazonaws.com/Blue+Nebula+2+-+1024x1024.png')] bg-repeat">
            <div className="flex flex-col min-h-screen">
                <div className="flex-grow">

                    <GameMenu forceClose={forceMenuClose} gameMethods={[restart, restore, save]}/>

                    <QueryClientProvider client={queryClient}>

                        <Game
                            restartGame={isRestarting}
                            serverText={serverText}
                            onRestoreDone={() => setRestoreGameId(undefined)}
                            restoreGameId={restoreGameId}
                            gaveSaved={gameSaved}
                            openRestoreModal={restore}
                            openSaveModal={save}
                            openRestartModal={restore}
                            onRestartDone={() => {
                                setConfirmRestartOpen(false);
                                setMenuForceClose(true);
                            }}/>


                        <ConfirmDialog
                            title="Restart Your Game? Are you sure? "
                            open={confirmOpen}
                            setOpen={setConfirmRestartOpen}
                            onConfirm={handleRestartGameConfirmClose}
                        />

                        <RestoreModal games={availableSavedGames} open={restoreDialogOpen}
                                      handleClose={handleRestoreModalClose} onDelete={handleDeleteGame}/>

                        <SaveModal games={availableSavedGames} open={saveDialogOpen}
                                   handleClose={handleSaveModalClose}/>

                    </QueryClientProvider>


                </div>
                <footer className="bg-gray-200 py-2">
                    <p className={"text-center text-black"}><a target="_blank"
                                                               href="https://github.com/arsindelve/ZorkAI">Created
                        By Mike in Dallas. Check out the repository.</a></p>
                </footer>
            </div>
        </div>
    );
}

export default App;

