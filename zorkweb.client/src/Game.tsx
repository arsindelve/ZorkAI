import {useMutation} from "@tanstack/react-query";
import {GameRequest, GameResponse, SessionHandler, Mixpanel, VerbsButton, CommandsButton, InventoryButton, LocationButton, DialogType} from "@zork-ai/shared-types";
import React, {useEffect, useState} from "react";
import {Alert, Button, CircularProgress, Snackbar} from "@mui/material";
import '@fontsource/roboto';
import Header from "./components/Header.tsx";

import Server from './Server';
import {ClickableText, ClickableTextHandle} from "@zork-ai/shared-types";
import Compass from "./components/Compass.tsx";

import {useGameContext} from "@zork-ai/shared-types";
import GameInput from "./components/GameInput.tsx";

// --- Per-word hover highlight (CSS Custom Highlight API) ---------------------
// Lives in the client (passed to ClickableText as onMouseMove/onMouseLeave) rather
// than inside the shared ClickableText, so it loads reliably regardless of how the
// shared package resolves. Highlights only the single word under the cursor to
// signal that individual words are clickable.
const WORD_HOVER_HIGHLIGHT = "word-hover";

const supportsHighlightApi = (): boolean =>
    typeof CSS !== "undefined" &&
    !!CSS.highlights &&
    typeof (globalThis as { Highlight?: unknown }).Highlight !== "undefined";

const expandToWordRange = (node: Node | null, offset: number): Range | null => {
    if (!node || node.nodeType !== Node.TEXT_NODE) return null;
    const text = node.textContent ?? "";
    if (!text) return null;
    let start = offset;
    let end = offset;
    while (start > 0 && !/\s/.test(text[start - 1])) start--;
    while (end < text.length && !/\s/.test(text[end])) end++;
    if (start === end) return null;
    const range = document.createRange();
    range.setStart(node, start);
    range.setEnd(node, end);
    return range;
};

const clearWordHighlight = (): void => {
    if (!supportsHighlightApi()) return;
    CSS.highlights.delete(WORD_HOVER_HIGHLIGHT);
};

const highlightWordAtPointer = (event: React.MouseEvent<HTMLDivElement>): void => {
    let node: Node | null = null;
    let offset = 0;
    const doc = document as Document & {
        caretPositionFromPoint?: (x: number, y: number) => { offsetNode: Node; offset: number } | null;
    };
    if (typeof doc.caretRangeFromPoint === "function") {
        const range = doc.caretRangeFromPoint(event.clientX, event.clientY);
        if (range) { node = range.startContainer; offset = range.startOffset; }
    } else if (typeof doc.caretPositionFromPoint === "function") {
        const pos = doc.caretPositionFromPoint(event.clientX, event.clientY);
        if (pos) { node = pos.offsetNode; offset = pos.offset; }
    }
    const wordRange = expandToWordRange(node, offset);
    const overWord = !!wordRange && wordRange.toString().trim().length > 0;

    // Pointer cursor only while hovering an actual (clickable) word, not whitespace.
    event.currentTarget.style.cursor = overWord ? "pointer" : "";

    if (!supportsHighlightApi()) return;
    if (!overWord) { clearWordHighlight(); return; }
    const HighlightCtor = (globalThis as { Highlight?: new (range: Range) => unknown }).Highlight!;
    // @ts-expect-error - highlights is not in older TS lib.dom typings
    CSS.highlights.set(WORD_HOVER_HIGHLIGHT, new HighlightCtor(wordRange!));
};

function Game() {

    const restoreResponse = "<Restore>";
    const saveResponse = "<Save>";
    const restartResponse = "<Restart>";

    const [playerInput, setInput] = useState<string>("");
    const [commandHistory, setCommandHistory] = useState<string[]>([]);
    const [gameText, setGameText] = useState<string[]>(["Your game is loading...."]);
    const [score, setScore] = useState<string>("0");
    const [moves, setMoves] = useState<string>("0");
    const [inventory, setInventory] = useState<string[]>([]);
    const [inventoryActions, setInventoryActions] = useState<Record<string, string[]>>({});
    const [locationActions, setLocationActions] = useState<Record<string, string[]>>({});
    const [exits, setExits] = useState<string[]>([]);
    const [locationName, setLocationName] = useState<string>("");

    const [snackBarOpen, setSnackBarOpen] = useState<boolean>(false);
    const [snackBarMessage, setSnackBarMessage] = useState<string>("");

    const sessionId = new SessionHandler();
    const server = new Server();

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
        // eslint-disable-next-line react-hooks/exhaustive-deps
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
        // eslint-disable-next-line react-hooks/exhaustive-deps
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
        // eslint-disable-next-line react-hooks/exhaustive-deps
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
        // eslint-disable-next-line react-hooks/exhaustive-deps
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
        // eslint-disable-next-line react-hooks/exhaustive-deps
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

        // Replace newline chars with HTML line breaks and preserve leading whitespace (spaces and tabs)
        data.response = data.response
            .replace(/\t/g, '    ')  // Convert tabs to 4 spaces
            .replace(/\n/g, "<br />")
            .replace(/^( +)/gm, (match) => '&nbsp;'.repeat(match.length))
            .replace(/<br \/>( +)/g, (_, spaces) => '<br />' + '&nbsp;'.repeat(spaces.length));

        const textToAppend = `<p class="text-lime-600 font-extrabold mt-3 mb-3">`
            + (!playerInput ? "" : `> ${playerInput}`) + `</p>`
            + data.response;

        setGameText((prevGameText) => [...prevGameText, textToAppend]);
        setInput("");
        setLocationName(data.locationName);
        setScore(data.score.toString());
        setMoves(data.moves.toString());
        setInventory(data.inventory);
        setInventoryActions(data.actionsAvailableFromInventory ?? {});
        setLocationActions(data.actionsAvailableFromLocation ?? {});
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
        // Record non-empty commands for Up/Down recall, collapsing immediate repeats.
        if (valueToSubmit) {
            setCommandHistory((prev) =>
                prev[prev.length - 1] === valueToSubmit ? prev : [...prev, valueToSubmit]);
        }
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
            md:block
            absolute
            top-24
            right-5
            opacity-90
            z-20
            pointer-events-auto
            "/>

            <ClickableText ref={gameContentElement} exits={exits} onWordClick={(word: string) => handleWordClicked(word)}
                           onMouseMove={highlightWordAtPointer}
                           onMouseLeave={clearWordHighlight}
                           className={"flex flex-col p-6 sm:p-12 bg-opacity-80 h-[65vh] overflow-auto " +
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

                {/* mt-auto pins the transcript to the bottom of the panel (terminal feel)
                    while still scrolling normally once the content overflows. */}
                <div className="mt-auto relative z-10 w-full">
                    {gameText.map((item: string, index: number) => (
                        <p
                            dangerouslySetInnerHTML={{__html: item}}
                            className={`mb-4 relative z-10 ${index === gameText.length - 1 ? 'animate-fadeIn' : ''}`}
                            key={index}
                            data-testid="game-response"
                        >
                        </p>
                    ))}
                </div>
            </ClickableText>

            <div
                className="flex flex-col items-stretch gap-2 bg-gradient-to-r from-stone-800 to-stone-700 px-3 sm:px-5 py-3 min-h-[90px] rounded-b-lg border-t border-stone-600/30 shadow-inner">
                {/* The command line is the primary interaction — give it its own full-width row. */}
                <GameInput
                    playerInputElement={playerInputElement}
                    isPending={mutation.isPending}
                    playerInput={playerInput}
                    setInput={setInput}
                    handleKeyDown={handleKeyDown}
                    commandHistory={commandHistory}
                />

                {mutation.isPending && (
                    <div className="p-2 flex items-center justify-center min-h-[44px]">
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
                        items-center
                        flex-wrap
                        gap-3 sm:gap-4
                        min-h-[44px]
                        ">
                        <VerbsButton onVerbClick={handleVerbClick}/>
                        {inventory.length > 0 && (
                            <InventoryButton
                                onInventoryClick={handleInventoryClick}
                                onActionClick={handleCommandClick}
                                inventory={inventory}
                                inventoryActions={inventoryActions}
                            />
                        )}
                        {Object.values(locationActions).some(actions => actions.length > 0) && (
                            <LocationButton
                                onItemClick={handleInventoryClick}
                                onActionClick={handleCommandClick}
                                locationActions={locationActions}
                            />
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
