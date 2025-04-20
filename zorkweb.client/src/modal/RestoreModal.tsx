import {ISavedGame} from "../model/SavedGame.ts";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import moment from 'moment';
import {useGameContext} from "../GameContext.tsx";
import RestoreIcon from '@mui/icons-material/Restore';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import SportsEsportsIcon from '@mui/icons-material/SportsEsports';
import { Typography, Paper, Box, Divider } from "@mui/material";

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
                sx={{
                    bgcolor: 'grey.900',
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
        </Dialog>
    );
}

export default RestoreModal;
