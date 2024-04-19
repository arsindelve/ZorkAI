import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./GameMenu.tsx";
import {ApplicationInsights} from '@microsoft/applicationinsights-web';
import {ReactPlugin} from '@microsoft/applicationinsights-react-js';
import { createContext, useState } from "react";


interface AppState {
    isRestarting: boolean
    isSaving: boolean
    isRestoring: boolean
    
    stopRestarting: ()=> void;
}

// State context
const AppStateContext = createContext<AppState | null>(null);

export { AppStateContext };

function App() {

    const [isRestarting, setIsRestarting] = useState<boolean>(false);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [isRestoring, setIsRestoring] = useState<boolean>(false);
    
 
    const queryClient = new QueryClient()

    var reactPlugin = new ReactPlugin();
    var appInsights = new ApplicationInsights({
        config: {
            connectionString: 'InstrumentationKey=249aa317-9170-4f53-8c39-ac618ebc77c5;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/;ApplicationId=b661a178-eb89-4a3f-bd5d-24d8ed8c9e04',
            enableAutoRouteTracking: true,
            extensions: [reactPlugin]
        }
    });
    appInsights.loadAppInsights();

    function restart(): void {
        setIsRestarting(!isRestarting)
    }

    function restore(): void {
        setIsRestoring(!setIsRestoring);
        console.log('Called restore from the child component');
    }

    function save(): void {
        setIsSaving(!isSaving);
        console.log('Called save from the child component');
    }
    
    const value = { isRestarting, isRestoring, isSaving, 
        
        stopRestarting: () => setIsRestarting(false) };
    
    return (
        <div
            className="bg-[url('https://zorkai-assets.s3.amazonaws.com/black-groove-stripes-repeating-background.jpg')] bg-repeat">
            <div className="flex flex-col min-h-screen">
                <div className="flex-grow">

                    <AppStateContext.Provider value={value}>
                    <GameMenu gameMethods={[restart, restore, save]}/>

                    <QueryClientProvider client={queryClient}>
                        <Game/>
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