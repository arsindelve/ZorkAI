import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import {Typography, Paper, Box, IconButton, Snackbar, Alert} from "@mui/material";
import React, {useState} from "react";
import ShareIcon from '@mui/icons-material/Share';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import {SessionHandler} from "../SessionHandler.ts";

interface ShareModalProps {
    open: boolean;
    setOpen: (open: boolean) => void;
}

function ShareModal(props: ShareModalProps) {
    const [copySuccess, setCopySuccess] = useState(false);
    const sessionHandler = new SessionHandler();
    const sessionId = sessionHandler.getSessionId()[0];

    const handleCopySessionId = async () => {
        try {
            await navigator.clipboard.writeText(sessionId);
            setCopySuccess(true);
        } catch (err) {
            console.error('Failed to copy session ID:', err);
        }
    };

    const handleClose = () => {
        props.setOpen(false);
        setCopySuccess(false);
    };

    return (
        <>
            <Dialog
                data-testid="share-game-modal"
                maxWidth="sm"
                open={props.open}
                fullWidth={true}
                aria-labelledby="share-dialog-title"
                PaperProps={{
                    style: {
                        borderRadius: '12px',
                        overflow: 'hidden'
                    }
                }}
            >
                <DialogTitle
                    id="share-dialog-title"
                    className="bg-gradient-to-r from-blue-600 to-blue-700"
                    sx={{
                        color: 'white',
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1,
                        py: 2
                    }}
                >
                    <ShareIcon fontSize="large"/>
                    <Typography variant="h5" component="span" fontWeight="bold">
                        Share Your Game
                    </Typography>
                </DialogTitle>

                <DialogContent sx={{pt: 3, pb: 1}}>
                    <Paper
                        elevation={2}
                        sx={{
                            p: 3,
                            borderRadius: '8px',
                            bgcolor: 'background.paper',
                            textAlign: 'center'
                        }}
                    >
                        <Typography variant="body1" sx={{mb: 3, color: 'grey.700'}}>
                            Share your Session ID with others so they can load copies of your saved games.
                        </Typography>

                        <Box 
                            sx={{
                                display: 'flex',
                                alignItems: 'center',
                                gap: 1,
                                p: 2,
                                bgcolor: 'grey.100',
                                borderRadius: '8px',
                                border: '2px solid',
                                borderColor: 'grey.300'
                            }}
                        >
                            <Typography 
                                variant="h6" 
                                sx={{
                                    fontFamily: 'monospace',
                                    fontSize: '1.2rem',
                                    color: 'grey.800',
                                    flex: 1,
                                    userSelect: 'all'
                                }}
                                data-testid="session-id-display"
                            >
                                {sessionId}
                            </Typography>
                            <IconButton
                                onClick={handleCopySessionId}
                                sx={{
                                    bgcolor: 'primary.main',
                                    color: 'white',
                                    '&:hover': {
                                        bgcolor: 'primary.dark'
                                    }
                                }}
                                data-testid="copy-session-id-button"
                            >
                                <ContentCopyIcon />
                            </IconButton>
                        </Box>

                        <Typography variant="body2" sx={{mt: 2, color: 'grey.600'}}>
                            Click the copy button to copy your Session ID to clipboard
                        </Typography>
                    </Paper>
                </DialogContent>

                <DialogActions sx={{p: 2, bgcolor: 'grey.100'}}>
                    <Button
                        onClick={handleClose}
                        variant="contained"
                        sx={{
                            borderRadius: '20px',
                            px: 3,
                            bgcolor: 'primary.main',
                            '&:hover': {
                                bgcolor: 'primary.dark'
                            }
                        }}
                    >
                        Close
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={copySuccess}
                autoHideDuration={3000}
                onClose={() => setCopySuccess(false)}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
            >
                <Alert 
                    onClose={() => setCopySuccess(false)} 
                    severity="success" 
                    sx={{ width: '100%' }}
                >
                    Session ID copied to clipboard!
                </Alert>
            </Snackbar>
        </>
    );
}

export default ShareModal;