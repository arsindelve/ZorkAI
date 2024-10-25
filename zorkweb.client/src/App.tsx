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
    //const [imageIsVisible, setImageIsVisible] = useState(true);

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
                        }}
                    />

                    <ConfirmDialog
                        title="Restart Your Game? Are you sure?"
                        open={confirmOpen}
                        setOpen={setConfirmRestartOpen}
                        onConfirm={handleRestartGameConfirmClose}
                    />

                    <RestoreModal games={availableSavedGames} open={restoreDialogOpen}
                                  handleClose={handleRestoreModalClose}/>

                    <SaveModal games={availableSavedGames} open={saveDialogOpen} handleClose={handleSaveModalClose}/>
                </QueryClientProvider>
            </div>

            <footer className="bg-gray-200 py-2">
                <p className="text-center text-sm text-black font-['Lato']">
                    <a target="_blank" href="https://github.com/arsindelve/ZorkAI">Created By Mike in Dallas. Check out
                        the repository.</a>
                </p>
            </footer>
        </div>


    );
}

export default App;


{/*<div className="App">*/
}
{/*    {imageIsVisible &&*/
}
{/*        <div className="fade">*/
}
{/*            <img src="https://zorkai-assets.s3.amazonaws.com/locations/WestOfHouse.webp" alt="description"*/
}
{/*                 onClick={() => setImageIsVisible(false)}/>*/
}
{/*        </div>*/
}
{/*    }*/
}
{/*</div>*/
}