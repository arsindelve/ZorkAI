import {useMutation} from "@tanstack/react-query";
import React, {useEffect, useState} from "react";
import {Alert, Button, CircularProgress, Snackbar} from "@mui/material";
import '@fontsource/roboto';
import {GameRequest} from "../model/GameRequest";
import {GameResponse} from "../model/GameResponse";
import {Header} from "./Header";
import {SessionHandler} from "../services/SessionHandler";
import {Server} from "../services/Server";
import {VerbsButton} from "./VerbsButton";
import {CommandsButton} from "./CommandsButton";
import {ClickableText, ClickableTextHandle} from "../utils/ClickableText";
import {Compass} from "./Compass";
import {Mixpanel} from "../services/Mixpanel";
import {InventoryButton} from "./InventoryButton";
import {DialogType} from "../model/DialogType";
import {GameInput} from "./GameInput";
import {useGameContext} from "../context/GameContext";

interface GameProps {
    baseUrl: string;
}

function Game({ baseUrl }: GameProps) {

    const restoreResponse = "<Restore>";
    const saveResponse = "<Save>";
    const restartResponse = "<Restart>";

    const [playerInput, setInput] = useState<string>("");
    const [gameText, setGameText] = useState<string[]>(["Your game is loading...."]);
    const [score, setScore] = useState<string>("0");
    const [moves, setMoves] = useState<string>("0");
    const [inventory, setInventory] = useState<string[]>([]);
    const [exits, setExits] = useState<string[]>([]);
    const [locationName, setLocationName] = useState<string>("");

    const [snackBarOpen, setSnackBarOpen] = useState<boolean>(false);
    const [snackBarMessage, setSnackBarMessage] = useState<string>("");

    const sessionId = new SessionHandler();
    const server = new Server(baseUrl);

    const gameContentElement = React.useRef<HTMLDivElement & ClickableTextHandle>(null);
    const playerInputElement = React.useRef<HTMLInputElement>(null);

    const {
        setDialogToOpen,
        restartGame,
        setRestartGame,
        saveGameRequest,
        setSaveGameRequest,
        setRestoreGameRequest,
        restoreGameRequest,
        deleteGameRequest,
        setDeleteGameRequest,
        setCopyGameTranscript
    } = useGameContext();

    function focusOnPlayerInput() {
        if (playerInputElement.current)
            window.setTimeout(() =>
                playerInputElement!.current!.focus(), 100);
    }

    // Save the game.
    useEffect(() => {
        if (saveGameRequest) {
            (async () => {
                saveGameRequest.sessionId = sessionId.getSessionId()[0];
                saveGameRequest.clientId = sessionId.getClientId();
                const response = await server.saveGame(saveGameRequest);
                setGameText((prevGameText) => [...prevGameText, response]);
                setSaveGameRequest(undefined);
                setSnackBarMessage("Game Saved Successfully.");
                setSnackBarOpen(true);
            })();
        }
        focusOnPlayerInput();
    }, [saveGameRequest]);

    // Restore a saved game
    useEffect(() => {
        if (!restoreGameRequest)
            return;
        setGameText([]);
        gameRestore(restoreGameRequest.id!).then((data) => {
            handleResponse(data);
            setRestoreGameRequest(undefined);
            focusOnPlayerInput();
        })
    }, [restoreGameRequest]);

    // Delete a saved game
    useEffect(() => {
        if (!deleteGameRequest)
            return;
        (async () => {
            try {
                await server.deleteSavedGame(deleteGameRequest.id!, sessionId.getClientId());
                setDeleteGameRequest(undefined);
                setSnackBarMessage("Game Deleted Successfully.");
                setSnackBarOpen(true);
                // Refresh the restore dialog to show updated list
                setDialogToOpen(DialogType.Restore);
            } catch (error) {
                console.error('Error deleting saved game:', error);
                setSnackBarMessage("Failed to delete game.");
                setSnackBarOpen(true);
            }
        })();
    }, [deleteGameRequest]);

    // Scroll to the bottom of the container after we add text.
    useEffect(() => {
        if (gameContentElement.current) {
            gameContentElement.current.scrollToBottom();
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
            setRestartGame(false);
            setSnackBarMessage("Game Restarted Successfully.");
            setSnackBarOpen(true);
            focusOnPlayerInput();
        })
    }, [restartGame]);

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
            setDialogToOpen(DialogType.Save);
            setInput("");
            return;
        }

        if (trimmed === restoreResponse) {
            setDialogToOpen(DialogType.Restore);
            setInput("");
            return;
        }

        if (trimmed === restartResponse) {
            setDialogToOpen(DialogType.Restart);
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
        setMoves(data.moves.toString());
        setInventory(data.inventory);
        setExits(data.exits);
    }

    // noinspection JSUnusedGlobalSymbols
    const mutation = useMutation({
        mutationFn: server.gameInput,
        onSuccess: (response) => {
            handleResponse(response);
        },
        onError: () => {
            // Error handling is done via the Alert component below
        }
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

    async function gameInit(): Promise<GameResponse> {
        const [id, firstTime] = sessionId.getSessionId();
        if (firstTime)
            setDialogToOpen(DialogType.Welcome);
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

    async function handleCopyToClipboard() {
        if (gameContentElement.current) {
            const success = await gameContentElement.current.copyToClipboardAsRTF();
            if (success) {
                setSnackBarMessage("Game text copied to clipboard with formatting.");
                setSnackBarOpen(true);
                Mixpanel.track('Copy to Clipboard', {});
            } else {
                setSnackBarMessage("Failed to copy text to clipboard.");
                setSnackBarOpen(true);
            }
        }
    }

    // Set the copyGameTranscript function in the context
    useEffect(() => {
        setCopyGameTranscript(() => handleCopyToClipboard);
    }, [setCopyGameTranscript]);

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

            <Header locationName={locationName} moves={moves} score={score}/>

            <Compass
            onCompassClick={handleCommandClick}
            exits={exits}
            className="
            hidden
            cursor: pointer
            md:block
            absolute
            top-24
            right-5
            w-[10%]
            h-auto
            opacity-75
            z-20
            pointer-events-auto
            "/>

            <ClickableText ref={gameContentElement} exits={exits} onWordClick={(word) => handleWordClicked(word)}
                           className={"p-6 sm:p-12 bg-opacity-80 h-[65vh] overflow-auto " +
                               "bg-stone-900 font-mono rounded-lg border-2 " +
                               "border-stone-700/50 shadow-lg z-10"}
                           data-testid="game-responses-container">
                <div className="relative z-0">
                    {/* Background styling elements */}
                    <div
                        className="absolute top-0 left-0 w-full h-full bg-[url('data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI1IiBoZWlnaHQ9IjUiPgo8cmVjdCB3aWR0aD0iNSIgaGVpZ2h0PSI1IiBmaWxsPSIjMjEyMTIxIj48L3JlY3Q+CjxwYXRoIGQ9Ik0wIDVMNSAwWk02IDRMNCA2Wk0tMSAxTDEgLTFaIiBzdHJva2U9IiMxYTFhMWEiIHN0cm9rZS13aWR0aD0iMSI+PC9wYXRoPgo8L3N2Zz4=')] opacity-5 pointer-events-none"></div>
                    <div
                        className="absolute top-2 left-2 w-20 h-20 rounded-full bg-lime-500/10 blur-3xl pointer-events-none"></div>
                    <div
                        className="absolute bottom-10 right-5 w-32 h-32 rounded-full bg-emerald-500/5 blur-3xl pointer-events-none"></div>
                </div>

                {gameText.map((item: string, index: number) => (
                    <p
                        dangerouslySetInnerHTML={{__html: item}}
                        className={`mb-4 relative z-10 ${index === gameText.length - 1 ? 'animate-fadeIn' : ''}`}
                        key={index}
                        data-testid="game-response"
                    >
                    </p>
                ))}
            </ClickableText>

            <div
                className="flex flex-wrap sm:flex-nowrap items-center justify-center gap-1 sm:gap-2 bg-gradient-to-r from-stone-800 to-stone-700 py-2 min-h-[90px] rounded-b-lg border-t border-stone-600/30 shadow-inner">
                <GameInput
                    playerInputElement={playerInputElement}
                    isPending={mutation.isPending}
                    playerInput={playerInput}
                    setInput={setInput}
                    handleKeyDown={handleKeyDown}
                />

                {mutation.isPending && (
                    <div className="mr-4 p-2 flex items-center justify-center">
                        <CircularProgress size={28} sx={{
                            color: '#84cc16',
                            boxShadow: '0 0 15px 5px rgba(132, 204, 22, 0.2)',
                            borderRadius: '50%'
                        }}/>
                    </div>
                )}

                {!mutation.isPending && (
                    <div
                        className="
                        flex
                        flex-row
                        justify-center
                        flex-wrap
                        sm:ml-2
                        sm:mr-4
                        gap-3 sm:gap-4
                        p-3
                        ">
                        <VerbsButton onVerbClick={handleVerbClick}/>
                        {inventory.length > 0 && (
                            <InventoryButton onInventoryClick={handleInventoryClick} inventory={inventory}/>
                        )}
                        <CommandsButton onCommandClick={handleCommandClick}/>

                        <Button
                            variant="contained"
                            color="primary"
                            size="large"
                            onClick={() => {
                                Mixpanel.track('Click Go', {});
                                submitInput();
                            }}
                            disabled={!playerInput}
                            sx={{
                                fontWeight: 'bold',
                                minWidth: '80px',
                                padding: '4px 10px',
                                backgroundColor: '#84cc16',
                                borderRadius: '8px',
                                transition: 'all 0.3s ease',

                            }}
                            data-testid="go-button">
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
