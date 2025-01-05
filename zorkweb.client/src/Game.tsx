import {useMutation} from "@tanstack/react-query";
import {GameRequest} from "./model/GameRequest.ts";
import {GameResponse} from "./model/GameResponse.ts";
import React, {useEffect, useState} from "react";
import {Alert, Button, CircularProgress, Snackbar} from "@mui/material";
import '@fontsource/roboto';
import Header from "./components/Header.tsx";
import {SessionHandler} from "./SessionHandler.ts";
import WelcomeDialog from "./modal/WelcomeModal.tsx";
import Server from './Server';
import VerbsButton from "./components/VerbsButton.tsx";
import CommandsButton from "./components/CommandsButton.tsx";
import ClickableText from "./ClickableText.tsx";
import Compass from "./components/Compass.tsx";
import {Mixpanel} from "./Mixpanel.ts";
import VideoDialog from "./modal/VideoModal.tsx";
import {useGameContext} from "./GameContext";
import InventoryButton from "./components/InventoryButton.tsx";

interface GameProps {
    restoreGameId?: string | undefined
    restartGame: boolean
    serverText: string
    onRestoreDone: () => void
    onRestartDone: () => void
    openRestoreModal: () => void
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
                  openRestartModal

              }: GameProps) {

    const restoreResponse = "<Restore>\n";
    const saveResponse = "<Save>\n"
    const restartResponse = "<Restart>\n"

    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>(["Your game is loading...."]);
    const [score, setScore] = useState<string>("0");
    const [moves, setMoves] = useState<string>("0");
    const [inventory, setInventory] = useState<string[]>([])
    const [locationName, setLocationName] = useState<string>("");
    const [welcomeDialogOpen, setWelcomeDialogOpen] = useState<boolean>(false);
    const [videoDialogOpen, setVideoDialogOpen] = useState<boolean>(false);
    const [snackBarOpen, setSnackBarOpen] = useState<boolean>(false);
    const [snackBarMessage, setSnackBarMessage] = useState<string>("");

    const sessionId = new SessionHandler();
    const server = new Server();

    const gameContentElement = React.useRef<HTMLDivElement>(null);
    const playerInputElement = React.useRef<HTMLInputElement>(null);

    const {dialogToOpen, setDialogToOpen} = useGameContext();

    function focusOnPlayerInput() {
        if (playerInputElement.current)
            window.setTimeout(() =>
                playerInputElement!.current!.focus(), 100);
    }

    useEffect(() => {

        if (!dialogToOpen)
            return;

        if (dialogToOpen === "Welcome") {
            setWelcomeDialogOpen(true);
            setDialogToOpen("");
        } else if (dialogToOpen === "Video") {
            setVideoDialogOpen(true);
            setDialogToOpen("");
        } 
    }, [dialogToOpen]);


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
            return
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

        if (data.response === saveResponse) {
            setDialogToOpen("Save")
            setInput("");
            return;
        }

        if (data.response === restoreResponse) {
            openRestoreModal();
            setInput("");
            return;
        }

        if (data.response === restartResponse) {
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
        setMoves(data.moves.toString())
        setInventory(data.inventory);
        console.log(inventory);
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

    function submitInput(inputValue?: string) {
        const [id] = sessionId.getSessionId();
        const valueToSubmit = (inputValue ?? playerInput).trim(); // Use parameter if provided, else fallback to state
        mutation.mutate(new GameRequest(valueToSubmit, id));
        focusOnPlayerInput();
    }

    function handleWordClicked(word: string) {
        setInput(playerInput + " " + word + " ");
        focusOnPlayerInput();
        Mixpanel.track('Click on Word', {
            "word": word
        });
    }

    const handleVerbClick = (verb: string) => {
        setInput(verb + " ");
        focusOnPlayerInput();
    };

    const handleInventoryClick = (item: string) => {
        setInput(playerInput + " " + item + " ");
        focusOnPlayerInput();
    };
    
    const handleCommandClick = (command: string) => {
        setInput(command);
        submitInput(command);
    };

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
        if (event.key === 'Enter') {
            Mixpanel.track('Press Enter', {});
            submitInput();
        }
    }

    const handleWelcomeDialogClose = () => {
        setWelcomeDialogOpen(false);
        Mixpanel.track('Close Welcome Dialog', {});
    };

    const handleWatchVideo = () => {
        Mixpanel.track('Open Video Dialog', {});
        setWelcomeDialogOpen(false);
        setVideoDialogOpen(true);
    }

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

        <div className={"m-10 mt-20 relative"}>

            <div>
                <Snackbar
                    anchorOrigin={{vertical: 'top', horizontal: 'center'}}
                    onClose={handleSnackbarClose}
                    open={snackBarOpen}
                    autoHideDuration={5000}
                    message={snackBarMessage}
                />
            </div>

            <VideoDialog open={videoDialogOpen} handleClose={() => setVideoDialogOpen(false)}/>
            <WelcomeDialog handleWatchVideo={handleWatchVideo} open={welcomeDialogOpen}
                           handleClose={handleWelcomeDialogClose}/>
            
            <Header locationName={locationName} moves={moves} score={score}/>

            <Compass onCompassClick={handleCommandClick} className="
            hidden
            cursor-pointer
            md:block
            absolute 
            top-16 
            right-1 
            w-1/5 
            h-auto
            "/>

            <ClickableText ref={gameContentElement} onWordClick={(word) => handleWordClicked(word)}
                           className={"p-12 bg-opacity-85 h-[65vh] overflow-auto bg-stone-900 font-mono "}>
                {gameText.map((item: string, index: number) => (
                    <p
                        dangerouslySetInnerHTML={{__html: item}}
                        className={"mb-4"}
                        key={index}
                    >
                    </p>
                ))}

            </ClickableText>

            <div className="flex flex-col sm:flex-row items-center bg-stone-700 min-h-[70px]">
                <input
                    ref={playerInputElement}
                    readOnly={mutation.isPending}
                    className="
                    w-full 
                    sm:p-4
                    m-1
                    p-1 
                    text-center 
                    sm:text-left 
                    focus:border-stone-500 
                    focus:border-[1.5px] 
                    focus:outline-none 
                    bg-stone-700"
                    value={playerInput}
                    placeholder="Type your command, then press enter/return."
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                ></input>

                {mutation.isPending && (
                    <div className="mr-4 bg-stone-700 p-2">
                        <CircularProgress size={25} sx={{color: 'white'}}/>
                    </div>
                )}

                {!mutation.isPending && (
                    <div
                        className="
                        flex 
                        flex-row 
                        justify-center 
                        sm:ml-4 
                        sm:mr-6 
                        mt-4 
                        mb-2 sm:mb-4 
                        space-x-6
                        ">
                        {inventory.length > 0 && (
                            <InventoryButton onInventoryClick={handleInventoryClick} inventory={inventory} />
                        )}
                        <CommandsButton onCommandClick={handleCommandClick}/>
                        <VerbsButton onVerbClick={handleVerbClick}/>
                        <Button
                            variant="contained"
                            onClick={() => {
                                Mixpanel.track('Click Go', {});
                                submitInput();
                            }}
                            disabled={!playerInput}>
                            Go
                        </Button>
                    </div>
                )}
            </div>

            {mutation.isError &&
                <Alert variant="filled" severity="error">Something went wrong with your
                    request. </Alert>}

        </div>
    )
}

export default Game;