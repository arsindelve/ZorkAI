import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import { Typography } from '@mui/material';

interface ConfirmationDialogProps {
    open: boolean;
    onConfirm: () => void;
    onCancel: () => void;
    title: string;
    message: string;
    confirmButtonText?: string;
    cancelButtonText?: string;
}

const ConfirmationDialog = ({
    open,
    onConfirm,
    onCancel,
    title,
    message,
    confirmButtonText = "Confirm",
    cancelButtonText = "Cancel"
}: ConfirmationDialogProps) => {
    return (
        <Dialog
            open={open}
            onClose={onCancel}
            aria-labelledby="confirmation-dialog-title"
            PaperProps={{
                style: {
                    borderRadius: '12px'
                }
            }}
        >
            <DialogTitle
                id="confirmation-dialog-title"
                sx={{
                    bgcolor: 'error.main',
                    color: 'error.contrastText',
                    py: 2
                }}
            >
                <Typography variant="h6" component="span" fontWeight="bold">
                    {title}
                </Typography>
            </DialogTitle>

            <DialogContent sx={{ pt: 3, pb: 2, px: 3 }}>
                <Typography variant="body1" sx={{ lineHeight: 1.6, p: 5 }}>
                    {message}
                </Typography>
            </DialogContent>

            <DialogActions sx={{ p: 2, gap: 1 }}>
                <Button
                    onClick={onCancel}
                    variant="outlined"
                    sx={{
                        borderRadius: '20px',
                        px: 3
                    }}
                >
                    {cancelButtonText}
                </Button>
                <Button
                    onClick={onConfirm}
                    variant="contained"
                    color="error"
                    sx={{
                        borderRadius: '20px',
                        px: 3
                    }}
                >
                    {confirmButtonText}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ConfirmationDialog;
