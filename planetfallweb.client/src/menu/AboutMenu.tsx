import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import config from "../../config.json";

export default function AboutMenu() {
    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
    };

    const go = (url: string) => {
        window.open(url, '_blank');
        handleClose();
    }

    return (
        <div>
            <Button
                id="basic-button"
                aria-controls={open ? 'basic-menu' : undefined}
                aria-haspopup="true"
                aria-expanded={open ? 'true' : undefined}
                onClick={handleClick}
            >
                About
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
                <MenuItem
                    onClick={() => go("https://github.com/arsindelve/ZorkAI?tab=readme-ov-file#all-the-greatness-of-the-original-zork-enhanced-with-ai")}>What
                    is Planetfall.AI?</MenuItem>
                <MenuItem onClick={() => go("https://infodoc.plover.net/manuals/planetfa.pdf")}>Read the Original
                    Infocom
                    Manual</MenuItem>
                <MenuItem onClick={() => go("https://infodoc.plover.net/maps/planetfa.pdf")}>Look at a Map
                    (spoilers)</MenuItem>
                <MenuItem onClick={() => go("http://www.eristic.net/games/infocom/planetfall.html")}>Look
                    at a Walkthrough (major spoilers)</MenuItem>
                <MenuItem onClick={() => go("https://en.wikipedia.org/wiki/Planetfall")}>Wikipedia Article on
                    Planetfall</MenuItem>
                <MenuItem>
                    <hr/>
                </MenuItem>
                <MenuItem>Version {config.version}</MenuItem>
            </Menu>
        </div>
    );
}
