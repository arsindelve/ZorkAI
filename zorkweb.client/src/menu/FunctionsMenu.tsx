import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';

interface FunctionsMenuProps {
    gameMethods: (() => void)[]
}

export default function FunctionsMenu({gameMethods}: FunctionsMenuProps) {

    const [restart, restore, save] = gameMethods;

    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
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
                Game Menu
            </Button>
            <Menu
                id="basic-menu"
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                MenuListProps={{
                    'aria-labelledby': 'basic-button',
                }}
            >
                <MenuItem onClick={() => restart()}>Restart Your Game</MenuItem>
                <MenuItem onClick={() => restore()}>Restore a Previous Saved Game</MenuItem>
                <MenuItem onClick={() => save()}>Save your Session</MenuItem>
            </Menu>
        </div>
    );
}