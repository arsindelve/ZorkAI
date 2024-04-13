import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "./Game.tsx";

function App() {
    
    const queryClient = new QueryClient()

    return (
        <div>
            <QueryClientProvider client={queryClient}>
                <h1 className={"text-3xl m-4"}>Zork AI</h1>
             
                <Game />
            </QueryClientProvider>
        </div>
    );
}

export default App;