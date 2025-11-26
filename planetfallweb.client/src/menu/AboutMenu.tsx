import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import config from "../../config.json";
import {Mixpanel, DialogType, useGameContext} from "@zork-ai/game-client-core";
import { ListItemIcon, ListItemText } from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import HelpIcon from '@mui/icons-material/Help';
import CodeIcon from '@mui/icons-material/Code';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import MapIcon from '@mui/icons-material/Map';
import ListAltIcon from '@mui/icons-material/ListAlt';
import ArticleIcon from '@mui/icons-material/Article';
import NewReleasesIcon from '@mui/icons-material/NewReleases';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';

export default function AboutMenu() {
    const {setDialogToOpen} = useGameContext();

    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const [latestVersion, setLatestVersion] = React.useState<string>(config.version);
    const open = Boolean(anchorEl);

    // Fetch latest version on mount
    React.useEffect(() => {
        const fetchLatestVersion = async () => {
            try {
                const response = await fetch('https://api.github.com/repos/arsindelve/ZorkAI/releases', {
                    headers: {
                        'Accept': 'application/vnd.github.v3+json'
                    }
                });
                if (response.ok) {
                    const releases = await response.json();
                    if (releases.length > 0) {
                        setLatestVersion(releases[0].tag_name || releases[0].name || config.version);
                    }
                }
            } catch (error) {
                console.error('Error fetching latest version:', error);
                // Keep using config.version as fallback
            }
        };
        fetchLatestVersion();
    }, []);

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
    };

    return (
        <div>
            <Button
                id="basic-button"
                data-testid="about-button"
                aria-controls={open ? 'basic-menu' : undefined}
                aria-haspopup="true"
                aria-expanded={open ? 'true' : undefined}
                onClick={handleClick}
                variant="contained"
                color="primary"
                startIcon={<InfoIcon />}
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
                <MenuItem
                    onClick={() => {
                        setDialogToOpen(DialogType.Welcome);
                        handleClose();
                    }}>
                    <ListItemIcon>
                        <HelpIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>What is Planetfall.AI?</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => go("Repo", "https://github.com/arsindelve/ZorkAI")}>
                    <ListItemIcon>
                        <CodeIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>See the source code</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => go("Planetfall Manual", "https://infodoc.plover.net/manuals/planetfa.pdf")}>
                    <ListItemIcon>
                        <MenuBookIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Read the Original Infocom Manual</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => go("Map", "https://infodoc.plover.net/maps/planetfa.pdf")}>
                    <ListItemIcon>
                        <MapIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Look at a Map (spoilers)</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => go("Walkthrough", "http://www.eristic.net/games/infocom/planetfall.html")}>
                    <ListItemIcon>
                        <ListAltIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Look at a Walkthrough (major spoilers)</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => go("Wikipedia", "https://en.wikipedia.org/wiki/Planetfall")}>
                    <ListItemIcon>
                        <ArticleIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Wikipedia Article on Planetfall</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => go("Playable Version", "https://archive.org/details/a2_Planetfall_1983_Infocom_a")}>
                    <ListItemIcon>
                        <PlayArrowIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Play the Original Infocom Version</ListItemText>
                </MenuItem>

                <MenuItem onClick={() => {
                    setDialogToOpen(DialogType.ReleaseNotes);
                    handleClose();
                }}>
                    <ListItemIcon>
                        <NewReleasesIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Version {latestVersion}</ListItemText>
                </MenuItem>
            </Menu>
        </div>
    );
}
