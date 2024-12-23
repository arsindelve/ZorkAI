import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import config from "../../config.json";
import {Mixpanel} from "../Mixpanel.ts";
import {useGameContext} from "../GameContext";

interface AboutMenuProps {

}

export default function AboutMenu({}: AboutMenuProps) {

    const {setDialogToOpen} = useGameContext();

    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };
    const handleClose = () => {
        setAnchorEl(null);
    };

    const go = (name: string, url: string) => {
        Mixpanel.track('Click on Menu Item', {
            "url": url,
            "name": name
        });
        window.open(url, '_blank');
        handleClose();
    }

    return (
        <div className={"text-xs  font-['Press_Start_2P']"}>
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
                    onClick={() => setDialogToOpen("Welcome")}>What is this game?</MenuItem>

                <MenuItem
                    onClick={() => setDialogToOpen("Video")}>Watch intro video</MenuItem>

                <MenuItem onClick={() => go("Repo", "https://github.com/arsindelve/ZorkAI")}>See the source code, visit
                    the repository</MenuItem>


                <MenuItem onClick={() => go("1984 Manual", "https://infodoc.plover.net/manuals/zork1.pdf")}>Read the
                    1984 Infocom
                    Manual</MenuItem>

                <MenuItem onClick={() => go("1982 Manual", "https://www.mocagh.org/infocom/zorkps-manual.pdf")}>Read the
                    1982 Radio
                    Shack TRS-80 Manual</MenuItem>

                <MenuItem onClick={() => go("Map", "https://www.mocagh.org/infocom/zork-map-front.pdf")}>Look at a Map
                    (spoilers)</MenuItem>

                <MenuItem
                    onClick={() => go("Walkthrough", "https://web.mit.edu/marleigh/www/portfolio/Files/zork/transcript.html")}>Look
                    at a Walkthrough (major spoilers!)</MenuItem>

                <MenuItem
                    onClick={() => go("Play Original", "https://iplayif.com/?story=https%3A%2F%2Feblong.com%2Finfocom%2Fgamefiles%2Fzork1-r119-s880429.z3")}>Play
                    the original Zork One</MenuItem>

                <MenuItem onClick={() => go("Wikipedia", "https://en.wikipedia.org/wiki/Zork")}>Wikipedia Article on
                    Zork</MenuItem>

                <MenuItem>Version {config.version}</MenuItem>

            </Menu>
        </div>
    );
}
