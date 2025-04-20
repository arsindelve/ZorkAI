import {ISavedGame} from "../model/SavedGame.ts";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import moment from 'moment';
import {useGameContext} from "../GameContext.tsx";

interface RestoreModalProps {
    open: boolean;
    setOpen: (open: boolean) => void;
    games: ISavedGame[]
}

function RestoreModal(props: RestoreModalProps) {

    const {setRestoreGameRequest} = useGameContext();

    function handleClose(item: ISavedGame | undefined) {
        console.log(item);
        setRestoreGameRequest(item);
        props.setOpen(false);
    }

    return (<Dialog
            data-testid="restore-game-modal"
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

                                <div key={game.id} className={"columns-3"}>

                                    <div
                                        className={"mb-2"}>
                                        {moment.utc(game.date).local().format('MMMM Do, h:mm a')}
                                    </div>

                                    <div className={"mb-4 text-left"}>
                                        {game.name}
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
        </Dialog>
    );
}

export default RestoreModal;
