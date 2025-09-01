import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./model/GameRequest.ts";
import {GameResponse} from "./model/GameResponse.ts";
import React, {useEffect, useState} from "react";
import {Alert, CircularProgress, Snackbar} from "@mui/material";
import '@fontsource/roboto';
import Header from "./Header.tsx";
import {SessionHandler} from "./SessionHandler.ts";
import WelcomeDialog from "./modal/WelcomeModal.tsx";
import Server from './Server';

interface GameProps {
    restoreGameId?: string | undefined
    restartGame: boolean
    serverText: string
    onRestoreDone: () => void
    onRestartDone: () => void
    openRestoreModal: () => void
    openSaveModal: () => void
    openRestartModal: () => void
    gaveSaved: boolean;
}

function Game({
                  restartGame,
                  restoreGameId,
                  serverText,
                  gaveSaved,
                  onRestoreDone,
                  onRestartDone,
                  openRestoreModal,
                  openSaveModal,
                  openRestartModal,
              }: GameProps) {

    const restoreResponse = "<Restore>";
    const saveResponse = "<Save>";
    const restartResponse = "<Restart>";

    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>(["Your game is loading...."]);
    const [score, setScore] = useState<string>("0");
    const [time, setTime] = useState<string>("0");
    const [locationName, setLocationName] = useState<string>("");
    const [welcomeDialogOpen, setWelcomeDialogOpen] = useState<boolean>(false);
    const [snackBarOpen, setSnackBarOpen] = useState<boolean>(false);
    const [snackBarMessage, setSnackBarMessage] = useState<string>("");

    const sessionId = new SessionHandler();
    const server = new Server();

    const gameContentElement = React.useRef<HTMLDivElement>(null);
    const playerInputElement = React.useRef<HTMLInputElement>(null);

    function focusOnPlayerInput() {
        if (playerInputElement.current)
            window.setTimeout(() =>
                playerInputElement!.current!.focus(), 100);
    }

    // Save the game. 
    useEffect(() => {
        if (gaveSaved) {
            gaveSaved = false;
            setSnackBarMessage("Game Saved Successfully.");
            setSnackBarOpen(true);
        }
        focusOnPlayerInput();
    }, [gaveSaved]);


    // Restore a saved game
    useEffect(() => {
        if (!restoreGameId)
            return;
        setGameText([]);
        gameRestore(restoreGameId!).then((data) => {
            handleResponse(data);
            onRestoreDone();
            focusOnPlayerInput();
        })
    }, [restoreGameId]);

    // Scroll to the bottom of the container after we add text. 
    useEffect(() => {
        if (gameContentElement.current) {
            gameContentElement.current.scrollTop = gameContentElement.current.scrollHeight;
        }
    }, [gameText]);

    // Restart the game. 
    useEffect(() => {
        if (!restartGame)
            return;
        sessionId.regenerate();
        setGameText([""]);
        gameInit().then((data) => {
            handleResponse(data);
            onRestartDone();
            focusOnPlayerInput();
        })
    }, [restartGame]);

    useEffect(() => {
        setGameText((prevGameText) => [...prevGameText, serverText]);
    }, [serverText]);

    // Set focus to the input box on load. 
    useEffect(() => {
        focusOnPlayerInput();
    }, []);

    // Load the initial text, either from the new session, or loading their old session. 
    useEffect(() => {
        gameInit().then((data) => {
            handleResponse(data);
        })
    }, []);


    function handleResponse(data: GameResponse) {
        const trimmed = (data.response ?? '').trim();
        if (trimmed === saveResponse) {
            openSaveModal();
            setInput("");
            return;
        }

        if (trimmed === restoreResponse) {
            openRestoreModal();
            setInput("");
            return;
        }

        if (trimmed === restartResponse) {
            openRestartModal();
            setInput("");
            return;
        }

        // Replace newline chars with HTML line breaks. 
        data.response = data.response.replace(/\n/g, "<br />");

        const textToAppend = `<p class="text-lime-600 font-extrabold mt-3 mb-3">`
            + (!playerInput ? "" : `> ${playerInput}`) + `</p>`
            + data.response;

        setGameText((prevGameText) => [...prevGameText, textToAppend]);
        setInput("");
        setLocationName(data.locationName);
        setScore(data.score.toString());
        setTime(data.time.toString())
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

    async function gameRestore(restoreGameId: string): Promise<GameResponse> {
        const [id] = sessionId.getSessionId();
        const response = server.gameRestore(restoreGameId, sessionId.getClientId(), id);
        setSnackBarMessage("Game Restored Successfully");
        setSnackBarOpen(true);
        return response;
    }

    function handleSnackbarClose() {
        setSnackBarOpen(false);
    }

    return (

        <div className={"m-8"}>

            <div>
                <Snackbar
                    anchorOrigin={{vertical: 'top', horizontal: 'center'}}
                    onClose={handleSnackbarClose}
                    open={snackBarOpen}
                    autoHideDuration={5000}
                    message={snackBarMessage}
                />
            </div>

            <WelcomeDialog open={welcomeDialogOpen} handleClose={handleWelcomeDialogClose}/>
            <Header locationName={locationName} time={time} score={score}/>

            <div ref={gameContentElement} className={"p-12 h-[65vh] overflow-auto bg-stone-900 font-mono"}>
                {gameText.map((item: string, index: number) => (
                    <p dangerouslySetInnerHTML={{__html: item}} className={"mb-4"} key={index}>
                    </p>
                ))}
            </div>

            <div className="flex items-center bg-neutral-700">
                <input ref={playerInputElement} readOnly={mutation.isPending}
                       className={"w-full p-4 focus:border-transparent focus:outline-none focus:ring-0 bg-neutral-700"}
                       value={playerInput} placeholder={"Enter your request then press enter / return."}
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