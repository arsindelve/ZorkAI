import './App.css';
import {QueryClient, QueryClientProvider, useMutation, useQuery} from '@tanstack/react-query'
import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from 'axios';
import {GameResponse} from "./GameResponse.ts";
import {GameRequest} from "./GameRequest.ts";
import {Forecast} from "./Forecast.ts";


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

    async function populateWeatherData(): Promise<Forecast[]> {
        const response = await fetch('weatherforecast');
        return response.json();
    }


    function Game() {

        const client = axios.create({
            baseURL: 'http://localhost:5223/ZorkOne/',
        });

        const config: AxiosRequestConfig = {
            headers: {
                'Accept': 'application/json',
            } as RawAxiosRequestHeaders,
        };

        // Queries
        const query = useQuery({queryKey: [], queryFn: populateWeatherData})

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
                <table className="table table-striped" aria-labelledby="tabelLabel">
                    <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                    </thead>

                    <tbody>
                    {
                        query.data?.map(forecast => {
                                return <tr key={forecast.date}>
                                    <td>{forecast.date}</td>
                                    <td>{forecast.temperatureC}</td>
                                    <td>{forecast.temperatureF}</td>
                                    <td>{forecast.summary}</td>
                                </tr>;
                            }
                        )}
                    </tbody>


                </table>
                <p>
                    <button
                        onClick={() => {
                            mutation.mutate(new GameRequest("hello"))
                        }}
                    >
                        Create Shit
                    </button>
                </p>
            </div>
        )

    }

}

export default App;