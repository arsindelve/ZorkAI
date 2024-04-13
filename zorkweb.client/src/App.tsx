import './App.css';
import {QueryClient, QueryClientProvider, useQuery} from '@tanstack/react-query'
import {Forecast} from "./Forecast.ts";

function App() {

    const queryClient = new QueryClient()

    return (

        <div>
            <h1 id="tabelLabel">Frobozz Forecast</h1>
            <p>What is the weather like this week in our Great Underground Empire?</p>
            <QueryClientProvider client={queryClient}>
                <Weather/>
            </QueryClientProvider>
        </div>

    );

    async function populateWeatherData(): Promise<Forecast[]> {
        const response = await fetch('weatherforecast');
        return response.json();
    }


    function Weather() {
        // Access the client
        //const queryClient = useQueryClient()

        // Queries
        const query = useQuery({queryKey: [], queryFn: populateWeatherData})

        // // Mutations
        // const mutation = useMutation({
        //     mutationFn: postTodo,
        //     onSuccess: () => {
        //         // Invalidate and refetch
        //         queryClient.invalidateQueries({queryKey: ['todos']})
        //     },
        // })

        return (

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
        );
    }

}

export default App;