import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";

function App() {

    const queryClient = new QueryClient()

    return (
        <div>


            <div className="flex flex-col min-h-screen">
                <div className="flex-grow">
                    <h1 className={"text-3xl m-4"}>Zork AI</h1>

                    <QueryClientProvider client={queryClient}>
                        <Game/>
                    </QueryClientProvider>

                </div>
                <footer className="bg-gray-200 py-2">
                    <p className={"text-center"}><a target="_blank" href="https://github.com/arsindelve/ZorkAI">Created
                        By Michael Lane. Check out the repo here.</a></p>
                </footer>
            </div>
        </div>
    );
}

export default App;