import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from "axios";
import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./GameRequest";
import {GameResponse} from "./GameResponse";
import {useState} from "react";
import {Alert, Button, Container, Paper} from "@mui/material";
import { TextareaAutosize  } from '@mui/base/TextareaAutosize';
import React from 'react';

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

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === 'Enter') {
            mutation.mutate(new GameRequest(input))
        }
    }
    
    async function gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {
        return await client.post<GameResponse, AxiosResponse>('', input, config);
    }

    return (
        <Container maxWidth="lg">
            <Paper elevation={3}>
                <TextareaAutosize className={"w-5/6 m-4"} readOnly={true} value={gameText} minRows={30}/>

                <input value={input}
                       onChange={(e) => setInput(e.target.value)}
                       onKeyDown={handleKeyDown}

                ></input>

                <div>
                    {mutation.isPending ? (
                        'Thinking...'
                    ) : (
                        <>
                            {mutation.isError ? (
                                <Alert variant="filled" severity="error">Something went wrong with your request. </Alert>
                            ) : null}

                            <Button variant={"contained"}
                                    onClick={() => {
                                        mutation.mutate(new GameRequest(input))
                                    }}
                            >
                                Do it.
                            </Button>

                        </>
                    )}
                </div>

            </Paper>

        </Container>
    )
}

export default Game;