import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import {useGameContext} from "../GameContext";
import DialogType from "../model/DialogType.ts";


export default function FunctionsMenu() {

    const {setDialogToOpen, copyGameTranscript} = useGameContext();
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
                id="basic-button"
                aria-controls={open ? 'basic-menu' : undefined}
                aria-haspopup="true"
                aria-expanded={open ? 'true' : undefined}
                onClick={handleClick}
            >
                Game
            </Button>
            <Menu
                id="basic-menu"
                anchorEl={anchorElement}
                open={open}
                onClose={handleClose}
                MenuListProps={{
                    'aria-labelledby': 'basic-button',
                }}
            >
                <MenuItem onClick={() => {
                    setDialogToOpen(DialogType.Restart);
                    handleClose();
                }}>Restart Your Game</MenuItem>
                <MenuItem onClick={() => {
                    setDialogToOpen(DialogType.Restore);
                    handleClose();
                }}>Restore a Previous Saved Game</MenuItem>
                <MenuItem onClick={() => {
                    setDialogToOpen(DialogType.Save);
                    handleClose();
                }}>Save your Game</MenuItem>
                <MenuItem onClick={() => {
                    copyGameTranscript();
                    handleClose();
                }}>Copy Game Transcript</MenuItem>
            </Menu>
        </div>
    );
}
