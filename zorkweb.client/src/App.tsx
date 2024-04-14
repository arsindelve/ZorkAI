import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./GameMenu.tsx";
import {ApplicationInsights} from '@microsoft/applicationinsights-web';
import {ReactPlugin} from '@microsoft/applicationinsights-react-js';

function App() {

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

    return (
        <div
            className="bg-[url('https://zorkai-assets.s3.amazonaws.com/black-groove-stripes-repeating-background.jpg')] bg-repeat">
            <div className="flex flex-col min-h-screen">
                <div className="flex-grow">

                    <GameMenu/>

                    <QueryClientProvider client={queryClient}>
                        <Game/>
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