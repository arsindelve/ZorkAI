import './App.css';
import {QueryClient, QueryClientProvider} from '@tanstack/react-query'
import Game from "../Game.tsx";

function App() {
    
    const queryClient = new QueryClient()

    return (
        <div>
            <QueryClientProvider client={queryClient}>
                <Game />
            </QueryClientProvider>
        </div>
    );
}

export default App;