import {ISavedGame} from "../model/SavedGame.ts";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import moment from 'moment';
import { IconButton } from "@mui/material";
import DeleteIcon from '@mui/icons-material/Delete';
import { useState } from "react";
import ConfirmDialog from "./ConfirmationDialog.tsx";

interface RestoreModalProps {
    open: boolean;
    handleClose: (id: string | undefined) => void;
    games: ISavedGame[]
    onDelete: (game: ISavedGame) => void;
}


function RestoreModal(props: RestoreModalProps) {
    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
    const [gameToDelete, setGameToDelete] = useState<ISavedGame | null>(null);

    function handleClose(item: ISavedGame | undefined) {
        props.handleClose(item?.id);
    }

    function handleDeleteClick(game: ISavedGame, event: React.MouseEvent) {
        event.stopPropagation();
        setGameToDelete(game);
        setDeleteConfirmOpen(true);
    }

    function handleDeleteConfirm() {
        if (gameToDelete) {
            props.onDelete(gameToDelete);
            setDeleteConfirmOpen(false);
            setGameToDelete(null);
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
                {"Restore A Previously Saved Game:"}
                <hr/>
            </DialogTitle>

            {!props.games.length && (

                <h3 className={"text-center m-10"}>You don't have any previously saved games.</h3>

            )}

            {props.games.length > 0 && (

                <DialogContent className={"max-h-60 overflow-auto"}>
                    <div>
                        <div>
                            {props.games.map((game) => (

                                <div key={game.id} className={"columns-4 items-center mb-4"}>

                                    <div className={"mb-2"}>
                                        {moment.utc(game.date).local().format('MMMM Do, h:mm a')}
                                    </div>

                                    <div className={"mb-2 text-left"}>
                                        {game.name}
                                    </div>

                                    <div className="text-center">
                                        <IconButton
                                            onClick={(e) => handleDeleteClick(game, e)}
                                            color="error"
                                            title="Delete saved game"
                                        >
                                            <DeleteIcon />
                                        </IconButton>
                                    </div>

                                    <div className="text-right">
                                        <Button size={"small"} variant="outlined"
                                                onClick={() => handleClose(game)}>Restore</Button>
                                    </div>

                                </div>
                            ))}
                        </div>
                    </div>
                </DialogContent>

            )}


            <DialogActions>
                <Button onClick={() => handleClose(undefined)} variant={"contained"}>
                    Cancel
                </Button>
            </DialogActions>
            
            <ConfirmDialog
                title="Delete Saved Game"
                open={deleteConfirmOpen}
                setOpen={setDeleteConfirmOpen}
                onConfirm={handleDeleteConfirm}
            />
        </Dialog>
    );
}

export default RestoreModal;