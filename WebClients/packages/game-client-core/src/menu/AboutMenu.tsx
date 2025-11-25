import * as React from 'react';
import Button from '@mui/material/Button';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import { Mixpanel } from "../services/Mixpanel.ts";
import DialogType from "../model/DialogType.ts";
import { ListItemIcon, ListItemText } from '@mui/material';
import InfoIcon from '@mui/icons-material/Info';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import HelpIcon from '@mui/icons-material/HelpOutline';
import PlayCircleOutlineIcon from '@mui/icons-material/PlayCircleOutline';
import NewReleasesIcon from '@mui/icons-material/NewReleases';

export interface AboutMenuItem {
    label: string;
    icon: React.ReactElement;
    action: 'dialog' | 'external' | 'version';
    dialogType?: DialogType;
    url?: string;
}

interface AboutMenuProps {
    menuItems: AboutMenuItem[];
    currentVersion: string;
    onDialogOpen: (dialogType: DialogType) => void;
}

export default function AboutMenu({ menuItems, currentVersion, onDialogOpen }: AboutMenuProps) {
    const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
    const [latestVersion, setLatestVersion] = React.useState<string>(currentVersion);
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
                        setLatestVersion(releases[0].tag_name || releases[0].name || currentVersion);
                    }
                }
            } catch (error) {
                console.error('Error fetching latest version:', error);
                // Keep using currentVersion as fallback
            }
        };
        fetchLatestVersion();
    }, [currentVersion]);

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleItemClick = (item: AboutMenuItem) => {
        if (item.action === 'dialog' && item.dialogType) {
            onDialogOpen(item.dialogType);
            handleClose();
        } else if (item.action === 'external' && item.url) {
            Mixpanel.track('Click on Menu Item', {
                "url": item.url,
                "name": item.label
            });
            window.open(item.url, '_blank');
            handleClose();
        } else if (item.action === 'version') {
            onDialogOpen(DialogType.ReleaseNotes);
            handleClose();
        }
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
                {menuItems.map((item, index) => (
                    <MenuItem
                        key={index}
                        onClick={() => handleItemClick(item)}
                    >
                        <ListItemIcon>
                            {item.icon}
                        </ListItemIcon>
                        <ListItemText>
                            {item.action === 'version' ? `Version ${latestVersion}` : item.label}
                        </ListItemText>
                    </MenuItem>
                ))}
            </Menu>
        </div>
    );
}

// Export commonly used icons for convenience
export {
    HelpIcon,
    PlayCircleOutlineIcon,
    NewReleasesIcon
};
