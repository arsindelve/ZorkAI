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

interface SaveModalProps {
    open: boolean;
    handleClose: (savedGame: ISaveGameRequest | undefined) => void;
    games: ISavedGame[]
}


function SaveModal(props: SaveModalProps) {

    const [newName, setNewName] = useState<string>("");

    function handleClose(savedGame: ISaveGameRequest | undefined) {
        props.handleClose(savedGame);
    }

    function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement | HTMLTextAreaElement>, savedGame: ISaveGameRequest) {
        if (event.key === 'Enter') {
            handleClose(savedGame);
        }
    }


    return (<Dialog

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

            {props.games.length > 0 && (

                <DialogContent className={"max-h-60 overflow-auto"}>
                    <h1 className={"mt-3 mb-3 "}>Overwrite a previously saved game: </h1>
                    <hr/>
                    <div className={"mt-5"}>
                        <div>
                            {props.games.map((game) => (

                                <div key={game.id} className={"columns-3"}>

                                    <div
                                        className={"mb-2"}>
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
                            ))}
                        </div>
                    </div>
                </DialogContent>

            )}


            <DialogActions>
                <Button onClick={() => handleClose(undefined)} variant="contained">
                    Cancel
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default SaveModal;