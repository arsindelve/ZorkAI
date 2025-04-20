import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';
import { Typography, Box } from '@mui/material';
import WarningIcon from '@mui/icons-material/Warning';

const ConfirmDialog = (props: any) => {
    const {title, children, message, open, setOpen, onConfirm, onCancel, dataTestId, confirmText = "Yes", cancelText = "No", confirmColor = "primary"} = props;
    return (
        <Dialog
            data-testid={dataTestId}
            open={open}
            fullWidth={true}
            maxWidth="sm"
            onClose={() => setOpen(false)}
            aria-labelledby="confirm-dialog"
            PaperProps={{
                style: {
                    borderRadius: '12px',
                    overflow: 'hidden'
                }
            }}
        >
            <DialogTitle 
                id="confirm-dialog"
                sx={{
                    bgcolor: 'grey.900',
                    color: 'white',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                    py: 2
                }}
            >
                <WarningIcon fontSize="large" />
                <Typography variant="h5" component="span" fontWeight="bold">
                    {title}
                </Typography>
            </DialogTitle>
            <DialogContent sx={{ p: 3, mt: 2 }}>
                <Typography variant="body1">
                    {children || message}
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
                >
                    {cancelText}
                </Button>
                <Button
                    variant="contained"
                    onClick={() => {
                        setOpen(false);
                        onConfirm();
                    }}
                    color={confirmColor}
                    sx={{ 
                        borderRadius: '20px',
                        px: 3
                    }}
                >
                    {confirmText}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ConfirmDialog;
