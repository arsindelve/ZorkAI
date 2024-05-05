import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./menu/GameMenu.tsx";
import {createContext, useState} from "react";
import Server from "./Server.ts";
import {SessionId} from "./SessionId.ts";
import RestoreModal from "./modal/RestoreModal.tsx";
import {ISavedGame} from "./model/SavedGame.ts";
import SaveModal from "./modal/SaveModal.tsx";
import {ISaveGameRequest} from "./model/SaveGameRequest.ts";


interface AppState {
    isRestarting: boolean
    stopRestarting: () => void;
}

// State context
const AppStateContext = createContext<AppState | null>(null);
export {AppStateContext};

function App() {

    const [restoreGameId, setRestoreGameId] = useState<string | undefined>(undefined);
    const [restoreDialogOpen, setRestoreDialogOpen] = useState<boolean>(false);
    const [saveDialogOpen, setSaveDialogOpen] = useState<boolean>(false);
    const [availableSavedGames, setAvailableSavedGames] = useState<ISavedGame[]>([]);
    const [isRestarting, setIsRestarting] = useState<boolean>(false);
    //const [imageIsVisible, setImageIsVisible] = useState(true);


    const server = new Server();
    const sessionId = new SessionId();
    const queryClient = new QueryClient();

    function restart(): void {
        setIsRestarting(!isRestarting)
    }

    async function restore(): Promise<void> {
        await getSavedGames();
        setRestoreDialogOpen(true);
    }

    async function getSavedGames() {
        const [id] = sessionId.getSessionId();
        const savedGames = await server.getSavedGames(id);
        setAvailableSavedGames(savedGames);
    }

    async function save(): Promise<void> {
        await getSavedGames();
        setSaveDialogOpen(true);
    }

    const value = {
        isRestarting,
        stopRestarting: () => setIsRestarting(false)
    };

    function handleRestoreModalClose(id: string | undefined): void {
        if (id)
            setRestoreGameId(id);
        setRestoreDialogOpen(false);
    }

    async function handleSaveModalClose(request: ISaveGameRequest | undefined): Promise<void> {
        if (request) {
            const [id] = sessionId.getSessionId();
            request.sessionId = id;
            await server.saveGame(request);
        }
        setSaveDialogOpen(false);
    }

    return (
        <div
            className="bg-[url('https://zorkai-assets.s3.amazonaws.com/black-groove-stripes-repeating-background.jpg')] bg-repeat">
            <div className="flex flex-col min-h-screen">
                <div className="flex-grow">

                    <AppStateContext.Provider value={value}>
                        <GameMenu gameMethods={[restart, restore, save]}/>

                        <QueryClientProvider client={queryClient}>

                            <Game restoreGameId={restoreGameId}/>

                            <RestoreModal games={availableSavedGames} open={restoreDialogOpen}
                                          handleClose={handleRestoreModalClose}/>

                            <SaveModal games={availableSavedGames} open={saveDialogOpen}
                                       handleClose={handleSaveModalClose}/>

                        </QueryClientProvider>
                    </AppStateContext.Provider>

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