import {ISavedGame} from "../model/SavedGame.ts";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogContentText from "@mui/material/DialogContentText";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import moment from 'moment';
import {ISaveGameRequest, SaveGameRequest} from "../model/SaveGameRequest.ts";
import {Input, Typography, Paper, Box, Divider, TextField} from "@mui/material";
import React, {useState} from "react";
import {useGameContext} from "../GameContext.tsx";
import SaveIcon from '@mui/icons-material/Save';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import SportsEsportsIcon from '@mui/icons-material/SportsEsports';

interface SaveModalProps {
    open: boolean;
    setOpen: (open: boolean) => void;
    games: ISavedGame[]
}


function SaveModal(props: SaveModalProps) {
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


    return (
        <Dialog
            data-testid="save-game-modal"
            maxWidth={"md"}
            open={props.open}
            fullWidth={true}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
            PaperProps={{
                style: { 
                    borderRadius: '12px',
                    overflow: 'hidden'
                }
            }}
        >
            <DialogTitle 
                id="alert-dialog-title"
                sx={{
                    bgcolor: 'primary.main',
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2
                }}
            >
                <SaveIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    Save Your Game
                </Typography>
            </DialogTitle>

            <DialogContent sx={{ pt: 3, pb: 1 }}>
                <DialogContentText sx={{ mb: 2, color: 'text.primary' }}>
                    Create a new save or overwrite an existing one to continue your adventure later.
                </DialogContentText>

                <Paper 
                    elevation={2}
                    sx={{ 
                        p: 3, 
                        mb: 3, 
                        borderRadius: '8px',
                        bgcolor: 'background.paper'
                    }}
                >
                    <Typography variant="h6" fontWeight="bold" sx={{ mb: 2, color: 'primary.main' }}>
                        Create New Save
                    </Typography>

                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                        <TextField
                            autoFocus
                            label="Name your saved game"
                            variant="outlined"
                            fullWidth
                            inputProps={{ maxLength: 25 }}
                            value={newName}
                            onChange={(event) => setNewName(event.target.value)}
                            onKeyDown={(e) => handleKeyDown(e, new SaveGameRequest(newName, undefined))}
                            sx={{ flexGrow: 1 }}
                        />
                        <Button 
                            variant="contained" 
                            color="primary"
                            startIcon={<SaveIcon />}
                            onClick={() => handleClose(new SaveGameRequest(newName, undefined))}
                            disabled={!newName.trim()}
                            sx={{ 
                                borderRadius: '20px',
                                px: 3,
                                py: 1
                            }}
                        >
                            Save
                        </Button>
                    </Box>
                </Paper>
            </DialogContent>

            <DialogContent sx={{ pt: 0, maxHeight: '40vh', overflow: 'auto' }}>
                <Divider sx={{ mb: 3 }} />

                <Typography variant="h6" fontWeight="bold" sx={{ mb: 2, color: 'primary.main' }}>
                    Overwrite Existing Save
                </Typography>

                {props.games.length === 0 ? (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <SportsEsportsIcon sx={{ fontSize: 60, color: 'text.secondary', mb: 2 }} />
                        <Typography variant="body1" color="text.secondary">
                            No saved games found
                        </Typography>
                    </Box>
                ) : (
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        {props.games.map((game) => (
                            <Paper 
                                key={game.id} 
                                elevation={2} 
                                sx={{ 
                                    p: 2, 
                                    borderRadius: '8px',
                                    transition: 'all 0.2s',
                                    '&:hover': {
                                        transform: 'translateY(-2px)',
                                        boxShadow: 4
                                    }
                                }}
                            >
                                <Box sx={{ 
                                    display: 'flex', 
                                    justifyContent: 'space-between',
                                    alignItems: 'center'
                                }}>
                                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, flex: 1 }}>
                                        <Typography variant="h6" fontWeight="bold" sx={{ color: 'primary.main' }}>
                                            {game.name}
                                        </Typography>

                                        <Box sx={{ display: 'flex', alignItems: 'center', color: 'text.secondary' }}>
                                            <AccessTimeIcon fontSize="small" sx={{ mr: 1 }} />
                                            <Typography variant="body2">
                                                {moment.utc(game.date).local().format('MMMM Do, h:mm a')}
                                            </Typography>
                                        </Box>
                                    </Box>

                                    <Button 
                                        variant="outlined" 
                                        color="primary"
                                        onClick={() => handleClose({
                                            name: game.name,
                                            id: game.id,
                                            sessionId: undefined,
                                            clientId: undefined
                                        })}
                                        sx={{ 
                                            borderRadius: '20px',
                                            px: 2
                                        }}
                                    >
                                        Overwrite
                                    </Button>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                )}
            </DialogContent>

            <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
                <Button 
                    onClick={() => justClose()} 
                    variant="outlined"
                    sx={{ borderRadius: '20px', px: 3 }}
                >
                    Cancel
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default SaveModal;
