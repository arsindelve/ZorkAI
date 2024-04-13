import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from "axios";
import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./GameRequest";
import {GameResponse} from "./GameResponse";
import React, {useEffect, useState} from "react";
import {Alert, CircularProgress, Container, Paper, Typography} from "@mui/material";
import '@fontsource/roboto';

function Game() {

    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>([])
    const [score, setScore] = useState<string>("0")
    const [moves, setMoves] = useState<string>("0")
    const [locationName, setLocationName] = useState<string>("");
    
    const gameContentElement = React.useRef<HTMLDivElement>(null);
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

            let textToAppend = `<hr /><p class="text-indigo-900 font-extrabold mt-3 mb-3">> ${playerInput}</p>`
                + response.data.response;

            setGameText((prevGameText) => [...prevGameText, textToAppend]);
            setInput("");
            setLocationName(response.data.locationName)
            setScore(response.data.score.toString())
            setMoves(response.data.moves.toString())
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

        <Container maxWidth="xl">

            <Paper elevation={2} className={"m-8 "}>

                <div className="flex items-center bg-gray-900 p-3 text-white grid grid-cols-5 gap-4">
                    <div>{locationName}</div>
                    <div></div>
                    <div></div>
                    <div>Moves: {moves}</div>
                    <div>Score: {score}</div>
                </div>

                <Typography style={{fontFamily: 'Roboto'}}>
                    <div ref={gameContentElement} className={"p-10 h-[65vh] overflow-auto bg-gray-100"}>
                        {gameText.map((item: string, index: number) => (
                            <p dangerouslySetInnerHTML={{__html: item}} className={"mb-4"} key={index}>
                            </p>
                        ))}
                    </div>
                </Typography>

                <div className="flex items-center">
                    <input ref={playerInputElement}
                           className={"w-full p-4 focus:border-transparent focus:outline-none focus:ring-0"}
                           value={playerInput} placeholder={"Tell me what to do next, then press return."}
                           onChange={(e) => setInput(e.target.value)}
                           onKeyDown={handleKeyDown}

                    ></input>
                    {mutation.isPending && <div className="mr-4"><CircularProgress size={20}/></div>}

                </div>

                {mutation.isError &&
                    <Alert variant="filled" severity="error">Something went wrong with your
                        request. </Alert>}

            </Paper>

        </Container>
    )
}

export default Game;