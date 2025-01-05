import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Button from '@mui/material/Button';


const ConfirmDialog = (props: any) => {
    const {title, children, open, setOpen, onConfirm, onCancel} = props;
    return (
        <Dialog
            open={open} fullWidth={false}
            onClose={() => setOpen(false)}
            aria-labelledby="confirm-dialog"
        >
            <DialogTitle id="confirm-dialog">{title}</DialogTitle>
            <DialogContent className="p-10">{children}</DialogContent>
            <DialogActions>
                <Button
                    variant="contained"
                    onClick={() => {
                        setOpen(false);
                        onCancel();
                    }}
                    color="primary"
                >
                    No
                </Button>
                <Button
                    variant="contained"
                    onClick={() => {
                        setOpen(false);
                        onConfirm();
                    }}

                >
                    Yes
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ConfirmDialog;
