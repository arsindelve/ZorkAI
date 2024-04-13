import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from "axios";
import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./src/GameRequest";
import {GameResponse} from "./src/GameResponse";
import {useState} from "react";

function Game() {

    const [input, setInput] = useState("");
    const [response, setResponse] = useState("")

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
        onSuccess: (data) => {
            setResponse(response + data.data.response);
        },
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

            <textarea value={response}></textarea>

            <input value={input}
                   placeholder="input"
                   onChange={(e) => setInput(e.target.value)}></input>

            <div>
                {mutation.isPending ? (
                    'Processing...'
                ) : (
                    <>
                        {mutation.isError ? (
                            <div>An error occurred: {mutation.error.message}</div>
                        ) : null}

                        {mutation.isSuccess ? <div>Todo added!</div> : null}

                        <button
                            onClick={() => {
                                mutation.mutate(new GameRequest(input))
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

export default Game;