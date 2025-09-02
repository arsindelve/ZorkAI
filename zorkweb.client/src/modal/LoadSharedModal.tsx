import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import {Typography, Paper, Box, TextField, Alert, Collapse} from "@mui/material";
import React, {useState} from "react";
import ContentPasteIcon from '@mui/icons-material/ContentPaste';
import moment from 'moment';
import {ISharedGame} from "../model/SharedGame.ts";
import {ShareGameRequest} from "../model/ShareGameRequest.ts";
import {SessionHandler} from "../SessionHandler.ts";
import Server from "../Server.ts";
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import SportsEsportsIcon from '@mui/icons-material/SportsEsports';

interface LoadSharedModalProps {
    open: boolean;
    setOpen: (open: boolean) => void;
    onGameCopied: () => void;
}

function LoadSharedModal(props: LoadSharedModalProps) {
    const [sessionIdInput, setSessionIdInput] = useState("");
    const [sharedGames, setSharedGames] = useState<ISharedGame[]>([]);
    const [loading, setLoading] = useState(false);
    const [copying, setCopying] = useState<string | null>(null);
    const [error, setError] = useState<string>("");
    const [validationError, setValidationError] = useState<string>("");

    const sessionHandler = new SessionHandler();
    const server = new Server();

    const validateSessionId = (sessionId: string): boolean => {
        const regex = /^[A-Za-z0-9]{15}$/;
        return regex.test(sessionId);
    };

    const handleSessionIdChange = (value: string) => {
        setSessionIdInput(value);
        setValidationError("");
        setError("");
        
        if (value && !validateSessionId(value)) {
            setValidationError("Session ID must be exactly 15 alphanumeric characters");
        }
    };

    const handleLoadSharedGames = async () => {
        if (!sessionIdInput.trim()) {
            setValidationError("Please enter a Session ID");
            return;
        }

        if (!validateSessionId(sessionIdInput)) {
            setValidationError("Session ID must be exactly 15 alphanumeric characters");
            return;
        }

        setLoading(true);
        setError("");
        
        try {
            const games = await server.getSharedGames(sessionIdInput);
            setSharedGames(games);
            
            if (games.length === 0) {
                setError("No saved games found for this Session ID");
            }
        } catch (error) {
            console.error('Error loading shared games:', error);
            setError("Failed to load shared games. Please check the Session ID and try again.");
            setSharedGames([]);
        } finally {
            setLoading(false);
        }
    };

    const handleCopyGame = async (game: ISharedGame) => {
        setCopying(game.id);
        setError("");

        try {
            const targetSessionId = sessionHandler.getClientId();
            const request = new ShareGameRequest(sessionIdInput, targetSessionId, game.id);
            
            await server.copySharedGame(request);
            props.onGameCopied();
            handleClose();
        } catch (error) {
            console.error('Error copying shared game:', error);
            setError("Failed to copy the shared game. Please try again.");
        } finally {
            setCopying(null);
        }
    };

    const handleClose = () => {
        props.setOpen(false);
        setSessionIdInput("");
        setSharedGames([]);
        setError("");
        setValidationError("");
        setLoading(false);
        setCopying(null);
    };

    const handleKeyDown = (event: React.KeyboardEvent<HTMLDivElement>) => {
        if (event.key === 'Enter' && !validationError && sessionIdInput.trim()) {
            handleLoadSharedGames();
        }
    };

    return (
        <Dialog
            data-testid="load-shared-game-modal"
            maxWidth="md"
            open={props.open}
            fullWidth={true}
            aria-labelledby="load-shared-dialog-title"
            PaperProps={{
                style: {
                    borderRadius: '12px',
                    overflow: 'hidden'
                }
            }}
        >
            <DialogTitle
                id="load-shared-dialog-title"
                className="bg-gradient-to-r from-green-600 to-green-700"
                sx={{
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2
                }}
            >
                <ContentPasteIcon fontSize="large"/>
                <Typography variant="h5" component="span" fontWeight="bold">
                    Load Shared Game
                </Typography>
            </DialogTitle>

            <DialogContent sx={{pt: 3, pb: 1}}>
                <Paper
                    elevation={2}
                    sx={{
                        p: 3,
                        mb: 3,
                        borderRadius: '8px',
                        bgcolor: 'background.paper'
                    }}
                >
                    <Typography variant="body1" sx={{mb: 3, color: 'grey.700'}}>
                        Enter a Session ID to view and copy available saved games.
                    </Typography>

                    <Box sx={{display: 'flex', alignItems: 'flex-start', gap: 2, mb: 2}}>
                        <TextField
                            autoFocus
                            label="Session ID"
                            variant="outlined"
                            fullWidth
                            value={sessionIdInput}
                            onChange={(e) => handleSessionIdChange(e.target.value)}
                            onKeyDown={handleKeyDown}
                            error={!!validationError}
                            helperText={validationError || "15 alphanumeric characters"}
                            inputProps={{
                                maxLength: 15,
                                style: { fontFamily: 'monospace', fontSize: '1.1rem' }
                            }}
                            data-testid="session-id-input"
                        />
                        <Button
                            variant="contained"
                            onClick={handleLoadSharedGames}
                            disabled={loading || !sessionIdInput.trim() || !!validationError}
                            sx={{
                                borderRadius: '20px',
                                px: 3,
                                py: 1.5,
                                bgcolor: 'green.600',
                                '&:hover': {
                                    bgcolor: 'green.700'
                                }
                            }}
                            data-testid="load-shared-games-button"
                        >
                            {loading ? 'Loading...' : 'Load Games'}
                        </Button>
                    </Box>

                    <Collapse in={!!error}>
                        <Alert severity="error" sx={{mb: 2}}>
                            {error}
                        </Alert>
                    </Collapse>
                </Paper>

                {sharedGames.length > 0 && (
                    <Box sx={{maxHeight: '40vh', overflow: 'auto'}}>
                        <Typography variant="h6" fontWeight="bold" sx={{mb: 2, color: 'grey.800'}}>
                            Available Shared Games
                        </Typography>
                        
                        <Box sx={{display: 'flex', flexDirection: 'column', gap: 2}}>
                            {sharedGames.map((game) => (
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
                                    data-testid="shared-game-item"
                                >
                                    <Box sx={{
                                        display: 'flex',
                                        justifyContent: 'space-between',
                                        alignItems: 'center'
                                    }}>
                                        <Box sx={{display: 'flex', flexDirection: 'column', gap: 1, flex: 1}}>
                                            <Typography variant="h6" fontWeight="bold" sx={{color: 'grey.800'}}>
                                                {game.name}
                                            </Typography>

                                            <Box sx={{display: 'flex', alignItems: 'center', color: 'text.secondary'}}>
                                                <AccessTimeIcon fontSize="small" sx={{mr: 1}}/>
                                                <Typography variant="body2">
                                                    {moment(game.savedOn).format('MMMM Do, h:mm a')}
                                                </Typography>
                                            </Box>
                                        </Box>

                                        <Button
                                            variant="outlined"
                                            disabled={copying === game.id}
                                            onClick={() => handleCopyGame(game)}
                                            sx={{
                                                borderRadius: '20px',
                                                px: 2,
                                                borderColor: 'green.600',
                                                color: 'green.700',
                                                '&:hover': {
                                                    borderColor: 'green.700',
                                                    bgcolor: 'green.50'
                                                }
                                            }}
                                            data-testid={`copy-game-${game.id}`}
                                        >
                                            {copying === game.id ? 'Copying...' : 'Copy Game'}
                                        </Button>
                                    </Box>
                                </Paper>
                            ))}
                        </Box>
                    </Box>
                )}
            </DialogContent>

            <DialogActions sx={{p: 2, bgcolor: 'grey.100'}}>
                <Button
                    onClick={handleClose}
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

export default LoadSharedModal;