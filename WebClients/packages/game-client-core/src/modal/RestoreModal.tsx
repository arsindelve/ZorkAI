import {ISavedGame} from "../model/SavedGame.ts";
import { Dialog, DialogTitle, DialogContent, DialogActions, Button, Typography, Paper, Box, Divider, IconButton } from "@mui/material";
import moment from 'moment';
import { Restore as RestoreIcon, AccessTime as AccessTimeIcon, SportsEsports as SportsEsportsIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { useState } from "react";
import ConfirmationDialog from "./ConfirmationDialog.tsx";

interface RestoreModalProps {
    open: boolean;
    setOpen: (open: boolean) => void;
    games: ISavedGame[];
    onRestoreGame: (game: ISavedGame | undefined) => void;
    onDeleteGame: (game: ISavedGame) => void;
}

function RestoreModal(props: RestoreModalProps) {

    const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
    const [gameToDelete, setGameToDelete] = useState<ISavedGame | null>(null);

    function handleClose(item: ISavedGame | undefined) {
        console.log(item);
        props.onRestoreGame(item);
        props.setOpen(false);
    }

    function handleDeleteClick(game: ISavedGame, event: React.MouseEvent) {
        event.stopPropagation();
        setGameToDelete(game);
        setDeleteConfirmOpen(true);
    }

    function handleDeleteConfirm() {
        if (gameToDelete) {
            props.onDeleteGame(gameToDelete);
            setDeleteConfirmOpen(false);
            setGameToDelete(null);
        }
    }

    function handleDeleteCancel() {
        setDeleteConfirmOpen(false);
        setGameToDelete(null);
    }

    return (
        <Dialog
            data-testid="restore-game-modal"
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
                className="bg-gradient-to-r from-gray-800 to-gray-900"
                sx={{
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2
                }}
            >
                <RestoreIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    Restore A Previously Saved Game
                </Typography>
            </DialogTitle>

            {!props.games.length && (
                <Box sx={{ p: 4, textAlign: 'center' }}>
                    <SportsEsportsIcon sx={{ fontSize: 60, color: 'text.secondary', mb: 2 }} />
                    <Typography variant="h6" color="text.secondary">
                        You don't have any previously saved games.
                    </Typography>
                </Box>
            )}

            {props.games.length > 0 && (
                <DialogContent sx={{ maxHeight: '60vh', p: 3 }}>

                    <Divider sx={{ mb: 3 }} />

                    <Box data-testid="restore-game-list" sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        {props.games.map((game) => (
                            <Paper
                                data-testid="restore-game-item"
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
                                        <Typography variant="h6" fontWeight="bold" sx={{ color: 'grey.800' }}>
                                            {game.name}
                                        </Typography>

                                        <Box sx={{ display: 'flex', alignItems: 'center', color: 'text.secondary' }}>
                                            <AccessTimeIcon fontSize="small" sx={{ mr: 1 }} />
                                            <Typography variant="body2">
                                                {moment.utc(game.date).local().format('MMMM Do, h:mm a')}
                                            </Typography>
                                        </Box>
                                    </Box>

                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                        <IconButton
                                            onClick={(e) => handleDeleteClick(game, e)}
                                            sx={{
                                                color: 'error.main',
                                                '&:hover': {
                                                    bgcolor: 'error.light',
                                                    color: 'error.contrastText'
                                                }
                                            }}
                                            title="Delete saved game"
                                        >
                                            <DeleteIcon />
                                        </IconButton>

                                        <Button
                                            variant="contained"
                                            startIcon={<RestoreIcon />}
                                            onClick={() => handleClose(game)}
                                            sx={{
                                                borderRadius: '20px',
                                                px: 2,
                                                bgcolor: 'grey.800',
                                                '&:hover': {
                                                    bgcolor: 'grey.700'
                                                }
                                            }}
                                        >
                                            Restore
                                        </Button>
                                    </Box>
                                </Box>
                            </Paper>
                        ))}
                    </Box>
                </DialogContent>
            )}

            <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
                <Button
                    onClick={() => handleClose(undefined)}
                    variant="outlined"
                    sx={{
                        borderRadius: '20px',
                        px: 3,
                        borderColor: 'grey.600',
                        color: 'grey.800',
                        '&:hover': {
                            borderColor: 'grey.800',
                            bgcolor: 'grey.100'
                        }
                    }}
                >
                    Cancel
                </Button>
            </DialogActions>

            <ConfirmationDialog
                open={deleteConfirmOpen}
                onConfirm={handleDeleteConfirm}
                onCancel={handleDeleteCancel}
                title="Delete Saved Game"
                message={`Are you sure you want to delete "${gameToDelete?.name}"? This action cannot be undone.`}
                confirmButtonText="Delete"
                cancelButtonText="Cancel"
            />
        </Dialog>
    );
}

export default RestoreModal;
