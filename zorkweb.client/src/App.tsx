import './App.css';
import {QueryClient, QueryClientProvider, useMutation} from '@tanstack/react-query'
import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from 'axios';
import {GameResponse} from "./GameResponse.ts";
import {GameRequest} from "./GameRequest.ts";


function App() {

    const queryClient = new QueryClient()

    return (

        <div>
            <h1 id="tabelLabel">Frobozz Forecast</h1>
            <p>What is the weather like this week in our Great Underground Empire?</p>
            <QueryClientProvider client={queryClient}>
                <Game/>
            </QueryClientProvider>
        </div>

    );

    function Game() {

        const client = axios.create({
            baseURL: 'http://localhost:5223/ZorkOne/',
        });

        const config: AxiosRequestConfig = {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        };

        // Mutations
        const mutation = useMutation({
            mutationFn: gameInput,
        })

        async function gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {
            try {
                return await client.post<GameResponse, AxiosResponse>('', input, config);
            } catch (e) {
                throw e;
            }
        }

        return (
            <div>


                <div>
                    {mutation.isPending ? (
                        'Adding todo...'
                    ) : (
                        <>
                            {mutation.isError ? (
                                <div>An error occurred: {mutation.error.message}</div>
                            ) : null}

                            {mutation.isSuccess ? <div>Todo added!</div> : null}

                            <button
                                onClick={() => {
                                    mutation.mutate(new GameRequest("hello"))
                                }}
                            >
                                Create Shit
                            </button>
                        </>
                    )}
                </div>
            </div>
        )

    }

}

export default App;