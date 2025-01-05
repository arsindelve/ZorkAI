import * as React from 'react';
import {useEffect} from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import {useGameContext} from "../GameContext";

interface FunctionsMenuProps {
    gameMethods: (() => void)[]
    forceClose: boolean
}

export default function FunctionsMenu({gameMethods, forceClose}: FunctionsMenuProps) {

    const {setDialogToOpen} = useGameContext();
    
    useEffect(() => {
        if (forceClose)
            handleClose();
    }, [forceClose]);

    const [restart, restore] = gameMethods;

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
                <MenuItem onClick={() => restart()}>Restart Your Game</MenuItem>
                <MenuItem onClick={() => restore()}>Restore a Previous Saved Game</MenuItem>
                <MenuItem onClick={() => setDialogToOpen("Save")}>Save your Game</MenuItem>
            </Menu>
        </div>
    );
}