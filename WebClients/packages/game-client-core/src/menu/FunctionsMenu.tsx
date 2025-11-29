import * as React from 'react';
import { Button, Menu, MenuItem, ListItemIcon, ListItemText } from '@mui/material';
import {
    RestartAlt as RestartAltIcon,
    Restore as RestoreIcon,
    Save as SaveIcon,
    ContentCopy as ContentCopyIcon,
    SportsEsports as SportsEsportsIcon,
    KeyboardArrowDown as KeyboardArrowDownIcon
} from '@mui/icons-material';
import DialogType from "../model/DialogType.ts";

interface FunctionsMenuProps {
    onDialogOpen: (dialogType: DialogType) => void;
    onCopyTranscript: () => void;
}

export default function FunctionsMenu({ onDialogOpen, onCopyTranscript }: FunctionsMenuProps) {
    const [anchorElement, setAnchorElement] = React.useState<null | HTMLElement>(null);
    const open = Boolean(anchorElement);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorElement(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorElement(null);
    };

    return (
        <div>
            <Button
                id="game-button"
                data-testid="game-button"
                aria-controls={open ? 'game-menu' : undefined}
                aria-haspopup="true"
                aria-expanded={open ? 'true' : undefined}
                onClick={handleClick}
                variant="contained"
                color="primary"
                startIcon={<SportsEsportsIcon />}
                endIcon={<KeyboardArrowDownIcon />}
                sx={{
                    borderRadius: '20px',
                    backgroundColor: 'rgba(255, 255, 255, 0.1)',
                    color: 'white',
                    '&:hover': {
                        backgroundColor: 'rgba(255, 255, 255, 0.2)',
                    },
                    transition: 'all 0.3s ease',
                    textTransform: 'none',
                    fontWeight: 'bold',
                }}
            >
                Game
            </Button>
            <Menu
                id="game-menu"
                data-testid="game-menu"
                anchorEl={anchorElement}
                open={open}
                onClose={handleClose}
                MenuListProps={{
                    'aria-labelledby': 'game-button',
                }}
                slotProps={{
                    paper: {
                        elevation: 3,
                        sx: {
                            borderRadius: '12px',
                            mt: 1,
                            '& .MuiMenuItem-root': {
                                px: 2,
                                py: 1.5,
                                transition: 'background-color 0.2s',
                                '&:hover': {
                                    backgroundColor: 'rgba(0, 0, 0, 0.04)',
                                },
                            },
                        }
                    }
                }}
                transformOrigin={{ horizontal: 'center', vertical: 'top' }}
                anchorOrigin={{ horizontal: 'center', vertical: 'bottom' }}
            >
                <MenuItem onClick={() => {
                    onDialogOpen(DialogType.Restart);
                    handleClose();
                }}>
                    <ListItemIcon>
                        <RestartAltIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Restart Your Game</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => {
                    onDialogOpen(DialogType.Restore);
                    handleClose();
                }}>
                    <ListItemIcon>
                        <RestoreIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Restore a Previous Saved Game</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => {
                    onDialogOpen(DialogType.Save);
                    handleClose();
                }}>
                    <ListItemIcon>
                        <SaveIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Save your Game</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => {
                    onCopyTranscript();
                    handleClose();
                }}>
                    <ListItemIcon>
                        <ContentCopyIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Copy Game Transcript</ListItemText>
                </MenuItem>
            </Menu>
        </div>
    );
}
