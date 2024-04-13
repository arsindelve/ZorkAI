import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from "axios";
import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./GameRequest";
import {GameResponse} from "./GameResponse";
import {useState} from "react";

function Game() {

    const [input, setInput] = useState("");
    const [gameText, setGameText] = useState("")

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
        onSuccess: (response) => {
            setGameText(gameText + response.data.response);
            setInput("");
            console.log(response.data.locationName)
            console.log(response.data.score)
            console.log(response.data.moves)
        },
    })

    async function gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {
        return await client.post<GameResponse, AxiosResponse>('', input, config);
    }

    return (
        <div>

            <textarea readOnly={true} value={gameText}></textarea>

            <input value={input}
                   placeholder="input"
                   onChange={(e) => setInput(e.target.value)}></input>

            <div>
                {mutation.isPending ? (
                    'Thinking...'
                ) : (
                    <>
                        {mutation.isError ? (
                            <div>An error occurred: {mutation.error.message}</div>
                        ) : null}

                        <button
                            onClick={() => {
                                mutation.mutate(new GameRequest(input))
                            }}
                        >
                            Do it.
                        </button>
                    </>
                )}
            </div>
        </div>
    )
}

export default Game;