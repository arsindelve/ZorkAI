import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';

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
                Help / About
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
                    onClick={() => go("https://github.com/arsindelve/ZorkAI?tab=readme-ov-file#all-the-greatness-of-the-original-zork-and-planetfall-enhanced-with-ai")}>What
                    is this game?</MenuItem>
                <MenuItem onClick={() => go("https://infodoc.plover.net/manuals/zork1.pdf")}>Read the 1984 Infocom
                    Manual</MenuItem>
                <MenuItem onClick={() => go("https://www.mocagh.org/infocom/zorkps-manual.pdf")}>Read the 1982 Radio
                    Shack TRS-80 Manual</MenuItem>
                <MenuItem onClick={() => go("https://www.mocagh.org/infocom/zork-map-front.pdf")}>Look at a Map
                    (spoilers)</MenuItem>
                <MenuItem onClick={() => go("https://web.mit.edu/marleigh/www/portfolio/Files/zork/transcript.html")}>Look
                    at a Walkthrough (major spoilers!)</MenuItem>
                <MenuItem
                    onClick={() => go("https://iplayif.com/?story=https%3A%2F%2Feblong.com%2Finfocom%2Fgamefiles%2Fzork1-r119-s880429.z3")}>Play
                    the original Zork One</MenuItem>
                <MenuItem onClick={() => go("https://en.wikipedia.org/wiki/Zork")}>Wikipedia Article on Zork</MenuItem>
            </Menu>
        </div>
    );
}
