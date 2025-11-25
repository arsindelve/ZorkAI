import React from 'react';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import { Typography } from '@mui/material';
import WarningIcon from '@mui/icons-material/Warning';

interface RestartConfirmDialogProps {
    open: boolean;
    setOpen: (open: boolean) => void;
    onConfirm: () => void;
    onCancel?: () => void;
}

const RestartConfirmDialog: React.FC<RestartConfirmDialogProps> = ({
    open,
    setOpen,
    onConfirm,
    onCancel
}) => {
    return (
        <Dialog
            data-testid="restart-confirm-dialog"
            open={open}
            fullWidth={true}
            maxWidth="sm"
            onClose={() => setOpen(false)}
            aria-labelledby="restart-confirm-dialog"
            PaperProps={{
                style: {
                    borderRadius: '12px',
                    overflow: 'hidden'
                }
            }}
        >
            <DialogTitle
                id="restart-confirm-dialog"
                className="bg-gradient-to-r from-gray-800 to-gray-900"
                sx={{
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2
                }}
            >
                <WarningIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    Restart Your Game? Are you sure?
                </Typography>
            </DialogTitle>
            <DialogContent sx={{ p: 5, mt: 5, mb: 2 }}>
                <Typography variant="body1">
                    Your game will be reset to the beginning. Are you sure you want to restart?
                </Typography>
            </DialogContent>
            <DialogActions sx={{ p: 2, bgcolor: 'grey.100' }}>
                <Button
                    variant="outlined"
                    onClick={() => {
                        setOpen(false);
                        if (onCancel) onCancel();
                    }}
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
                    data-testid="restart-confirm-cancel"
                >
                    Cancel
                </Button>
                <Button
                    variant="contained"
                    onClick={() => {
                        setOpen(false);
                        onConfirm();
                    }}
                    color="error"
                    sx={{
                        borderRadius: '20px',
                        px: 3
                    }}
                    data-testid="restart-confirm-yes"
                >
                    Restart
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default RestartConfirmDialog;
