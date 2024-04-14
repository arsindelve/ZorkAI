import axios, {AxiosRequestConfig, AxiosResponse, RawAxiosRequestHeaders} from "axios";
import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./GameRequest";
import {GameResponse} from "./GameResponse";
import React, {useEffect, useState} from "react";
import {Alert, CircularProgress} from "@mui/material";
import '@fontsource/roboto';
import Header from "./Header.tsx";
import {SessionId} from "./SessionId.ts";

function Game() {

    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>([])
    const [score, setScore] = useState<string>("0")
    const [moves, setMoves] = useState<string>("0")
    const [locationName, setLocationName] = useState<string>("");

    const sessionId = new SessionId();

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

    // const { data, isError, isLoading }: UseQueryResult<AxiosResponse<GameResponse>> = useQuery({ queryKey: ['gameInit'], queryFn: gameInit });

    function handleResponse(data: GameResponse) {

        // Replace newline chars with HTML line breaks. 
        data.response = data.response.replace(/\n/g, "<br />");

        let textToAppend = `<p class="text-lime-600 font-extrabold mt-3 mb-3">> ${playerInput}</p>`
            + data.response;

        setGameText((prevGameText) => [...prevGameText, textToAppend]);
        setInput("");
        setLocationName(data.locationName)
        setScore(data.score.toString())
        setMoves(data.moves.toString())
    }

    const mutation = useMutation({
        mutationFn: gameInput,
        onSuccess: (response) => {
            handleResponse(response.data);
        },
    })

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === 'Enter') {
            mutation.mutate(new GameRequest(playerInput, sessionId.getSessionId()))
        }
    }

    async function gameInit(): Promise<AxiosResponse<GameResponse>> {
        const {data} = await axios.get<GameResponse, AxiosResponse>("?sessionId=" + sessionId.getSessionId());
        return data;
    }


    async function gameInput(input: GameRequest): Promise<AxiosResponse<GameResponse>> {
        return await client.post<GameResponse, AxiosResponse>('', input, config);
    }

    return (

        <div className={"m-12"}>

            <Header locationName={locationName} moves={moves} score={score}/>

            <div ref={gameContentElement} className={"p-12 h-[65vh] overflow-auto bg-stone-900"}>
                {gameText.map((item: string, index: number) => (
                    <p dangerouslySetInnerHTML={{__html: item}} className={"mb-4"} key={index}>
                    </p>
                ))}
            </div>


            <div className="flex items-center bg-neutral-700">
                <input ref={playerInputElement} readOnly={mutation.isPending}
                       className={"w-full p-4 focus:border-transparent focus:outline-none focus:ring-0 bg-neutral-700"}
                       value={playerInput} placeholder={"Tell me what you want to do next, then press return."}
                       onChange={(e) => setInput(e.target.value)}
                       onKeyDown={handleKeyDown}

                ></input>

                {mutation.isPending && <div className="mr-4 bg-neutral-700"><CircularProgress size={25}/></div>}

            </div>

            {mutation.isError &&
                <Alert variant="filled" severity="error">Something went wrong with your
                    request. </Alert>}

        </div>

    )
}

export default Game;