import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from "axios";
import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./GameRequest";
import {GameResponse} from "./GameResponse";
import React, {useEffect, useState} from "react";
import {Alert, Container, Paper} from "@mui/material";

function Game() {

    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>([])

    const gameContentElement = React.useRef<HTMLUListElement>(null);
    const playerInputElement = React.useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (gameContentElement.current) {
            gameContentElement.current.scrollTop = gameContentElement.current.scrollHeight;
        }
    }, [gameText]);

    useEffect(() => {
        if (playerInputElement.current)
            playerInputElement.current.focus();
    }, []);

    const client = axios.create({
        baseURL: import.meta.env.VITE_BASE_URL
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
            response.data.response = response.data.response.replace(/\n/g, "<br />");
            setGameText((prevGameText) => [...prevGameText, response.data.response]);
            setInput("");
            console.log(response.data.locationName)
            console.log(response.data.score)
            console.log(response.data.moves)
        },
    })

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === 'Enter') {
            mutation.mutate(new GameRequest(playerInput))
        }
    }

    async function gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {
        return await client.post<GameResponse, AxiosResponse>('', input, config);
    }

    return (
        <Container maxWidth="lg">
            <Paper elevation={3} className={"m-8"}>

                <ul ref={gameContentElement} className={"p-5 h-[65vh] overflow-auto"}>
                    {gameText.map((item: string, index: number) => (
                        <li className={"mb-4"} key={index}>{item}</li>
                    ))}
                </ul>

                <input ref={playerInputElement}
                       className={"w-full p-4 focus:border-transparent focus:outline-none focus:ring-0"}
                       value={playerInput} placeholder={"Tell me what to do next"}
                       onChange={(e) => setInput(e.target.value)}
                       onKeyDown={handleKeyDown}

                ></input>

                <div>
                    {mutation.isPending ? (
                        'Thinking...'
                    ) : (
                        <>
                            {mutation.isError ? (
                                <Alert variant="filled" severity="error">Something went wrong with your
                                    request. </Alert>
                            ) : null}
                        </>
                    )}
                </div>

            </Paper>

        </Container>
    )
}

export default Game;