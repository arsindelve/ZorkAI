import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./model/GameRequest.ts";
import {GameResponse} from "./model/GameResponse.ts";
import React, {useContext, useEffect, useState} from "react";
import {Alert, CircularProgress} from "@mui/material";
import '@fontsource/roboto';
import Header from "./Header.tsx";
import {SessionId} from "./SessionId.ts";
import WelcomeDialog from "./modal/WelcomeModal.tsx";
import Server from './Server';
import {AppStateContext} from "./App.tsx";
import ConfirmDialog from "./modal/ConfirmationDialog.tsx";

interface GameProps {
    restoreGameId?: string | undefined
}

function Game({restoreGameId}: GameProps) {

    const appState = useContext(AppStateContext);

    const [confirmOpen, setConfirmRestartOpen] = useState<boolean>(false);
    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>([]);
    const [score, setScore] = useState<string>("0");
    const [moves, setMoves] = useState<string>("0");
    const [locationName, setLocationName] = useState<string>("");
    const [welcomeDialogOpen, setWelcomeDialogOpen] = useState<boolean>(false);

    const sessionId = new SessionId();
    const server = new Server();

    const gameContentElement = React.useRef<HTMLDivElement>(null);
    const playerInputElement = React.useRef<HTMLInputElement>(null);

    useEffect(() => {
        console.log("Let's restore this game: " + restoreGameId);
    }, [restoreGameId]);

    // Scroll to the bottom of the container after we add text. 
    useEffect(() => {
        if (gameContentElement.current) {
            gameContentElement.current.scrollTop = gameContentElement.current.scrollHeight;
        }
    }, [gameText]);

    useEffect(() => {
        if (appState?.isRestarting)
            setConfirmRestartOpen(true);
        appState?.stopRestarting();
    }, [appState?.isRestarting]);

    // Set focus to the input box on load. 
    useEffect(() => {
        if (playerInputElement.current)
            playerInputElement.current.focus();
    }, []);

    // Load the initial text, either from the new session, or loading their old session. 
    useEffect(() => {
        gameInit().then((data) => {
            handleResponse(data);
        })
    }, []);

    function restartGame() {

        setConfirmRestartOpen(false);
        setGameText([]);
        sessionId.regenerate();
        gameInit().then((data) => {
            handleResponse(data);
        })
    }

    function handleResponse(data: GameResponse) {

        // Replace newline chars with HTML line breaks. 
        data.response = data.response.replace(/\n/g, "<br />");

        const textToAppend = `<p class="text-lime-600 font-extrabold mt-3 mb-3">`
            + (!playerInput ? "" : `> ${playerInput}`) + `</p>`
            + data.response;

        setGameText((prevGameText) => [...prevGameText, textToAppend]);
        setInput("");
        setLocationName(data.locationName);
        setScore(data.score.toString());
        setMoves(data.moves.toString())
    }

    // noinspection JSUnusedGlobalSymbols
    const mutation = useMutation({
        mutationFn: server.gameInput,
        onSuccess: (response) => {
            handleResponse(response);
        },
        onError: (r => {
            console.log(r);
        })
    });

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === 'Enter') {
            const [id] = sessionId.getSessionId();
            mutation.mutate(new GameRequest(playerInput, id))
        }
    }

    const handleWelcomeDialogClose = () => {
        setWelcomeDialogOpen(false);
    };

    async function gameInit(): Promise<GameResponse> {
        const [id, firstTime] = sessionId.getSessionId();
        setWelcomeDialogOpen(firstTime);
        return await server.gameInit(id)
    }

    return (

        <div className={"m-8"}>

            <ConfirmDialog
                title="Restart Your Game? Are you sure? "
                open={confirmOpen}
                setOpen={setConfirmRestartOpen}
                onConfirm={restartGame}
            />

            <WelcomeDialog open={welcomeDialogOpen} handleClose={handleWelcomeDialogClose}/>
            <Header locationName={locationName} moves={moves} score={score}/>

            <div ref={gameContentElement} className={"p-12 h-[65vh] overflow-auto bg-stone-900 font-mono"}>
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