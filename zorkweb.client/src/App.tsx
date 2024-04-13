import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";
import GameMenu from "./GameMenu.tsx";

function App() {

    const queryClient = new QueryClient()

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
                    <p className={"text-center"}>Created
                        By Mike in Dallas. <a target="_blank" href="https://github.com/arsindelve/ZorkAI">Check out the
                            repo here.</a></p>
                </footer>
            </div>
        </div>
    );
}

export default App;