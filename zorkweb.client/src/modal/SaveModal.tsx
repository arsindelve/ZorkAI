import {ISavedGame} from "../model/SavedGame.ts";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import moment from 'moment';
import {ISaveGameRequest, SaveGameRequest} from "../model/SaveGameRequest.ts";
import {Input} from "@mui/material";
import React, {useState} from "react";
import {useGameContext} from "../GameContext.tsx";

interface SaveModalProps {
    open: boolean;
    setOpen: (open: boolean) => void;
    games: ISavedGame[]
}


function SaveModal(props: SaveModalProps) {
    console.log('SaveModal rendered with props:', JSON.stringify(props));
    console.log('SaveModal games prop length:', props.games.length);

    const [newName, setNewName] = useState<string>("");

    const {setSaveGameRequest} = useGameContext();

    function handleClose(savedGame: ISaveGameRequest) {
        setSaveGameRequest(savedGame);
        props.setOpen(false);
    }

    function justClose() {
        props.setOpen(false);
    }

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>, savedGame: ISaveGameRequest) {
        if (event.key === 'Enter') {
            handleClose(savedGame);
        }
    }


    return (<Dialog
            data-testid="save-game-modal"
            maxWidth={"md"}
            open={props.open}
            fullWidth={true}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">
                {"Save Your Game"}
                <hr/>
            </DialogTitle>

            <DialogContent className={"max-h-60 overflow-auto"}>


                <div className={"columns-3"}>
                    <div className={"mt-3"}>Name your saved game:</div>
                    <div className={"w-full"}>
                        <Input autoFocus inputProps={{maxLength: 25}} className={"w-full"}
                               onKeyDown={(e) => handleKeyDown(e, new SaveGameRequest(newName, undefined))}
                               onChange={(event) => setNewName(event.target.value)}/></div>
                    <div className={"text-right"}><Button variant="outlined"
                                                          onClick={() => handleClose(new SaveGameRequest(newName, undefined))}>Save</Button>
                    </div>
                </div>
            </DialogContent>

            {/* Always render this section for testing purposes, but conditionally show saved games */}
            <DialogContent className={"max-h-60 overflow-auto"}>
                <h1 className={"mt-3 mb-3 "}>Overwrite a previously saved game: </h1>
                <hr/>
                <div className={"mt-5"}>
                    <div>
                        {props.games.length > 0 ? (
                            // If there are saved games, map and display them
                            props.games.map((game) => (
                                <div key={game.id} className={"columns-3"}>
                                    <div className={"mb-2"}>
                                        {moment.utc(game.date).local().format('MMMM Do, h:mm a')}
                                    </div>

                                    <div className={"mb-4 text-left"}>
                                        {game.name}
                                    </div>

                                    <div className="text-right">
                                        <Button variant="outlined" size="small"
                                                onClick={() => handleClose({
                                                    name: game.name,
                                                    id: game.id,
                                                    sessionId: undefined,
                                                    clientId: undefined
                                                })}>Overwrite</Button>
                                    </div>
                                </div>
                            ))
                        ) : (
                            // If there are no saved games, display a message
                            <div className={"columns-3"}>
                                <div className={"mb-2"}>No saved games found</div>
                                <div className={"mb-4 text-left"}></div>
                                <div className="text-right"></div>
                            </div>
                        )}
                    </div>
                </div>
            </DialogContent>


            <DialogActions>
                <Button onClick={() => justClose()} variant="contained">
                    Cancel
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default SaveModal;
